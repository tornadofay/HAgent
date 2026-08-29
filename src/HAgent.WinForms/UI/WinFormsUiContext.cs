using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HAgent.WinForms.UI
{
    public sealed class WinFormsUiContext : IUiContext, IDisposable
    {
        private readonly Form _form;
        private bool _disposed;

        public WinFormsUiContext(Form form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }

        public Form RootForm { get { return _form; } }

        public Task<UiControlSnapshot> InspectAsync(string controlId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return OnUiAsync(delegate
            {
                var control = string.IsNullOrWhiteSpace(controlId) ? (Control)_form : FindControl(_form, controlId);
                if (control == null) throw new ArgumentException("Control was not found: " + controlId, nameof(controlId));
                return BuildSnapshot(control, true);
            });
        }

        public Task<object> ReadControlAsync(string controlId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(controlId)) throw new ArgumentException("Control ID is required.", nameof(controlId));
            cancellationToken.ThrowIfCancellationRequested();
            return OnUiAsync(delegate
            {
                var control = FindControl(_form, controlId);
                if (control == null) throw new ArgumentException("Control was not found: " + controlId, nameof(controlId));
                return ReadValue(control);
            });
        }

        public Task<IReadOnlyList<IReadOnlyDictionary<string, object>>> ReadDataAsync(string controlId, int maxRows = 100, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(controlId)) throw new ArgumentException("Control ID is required.", nameof(controlId));
            if (maxRows < 1) throw new ArgumentOutOfRangeException(nameof(maxRows));
            cancellationToken.ThrowIfCancellationRequested();
            return OnUiAsync(delegate
            {
                var control = FindControl(_form, controlId);
                if (control == null) throw new ArgumentException("Control was not found: " + controlId, nameof(controlId));
                var grid = control as DataGridView;
                if (grid == null) throw new ArgumentException("Control is not a DataGridView: " + controlId, nameof(controlId));
                return ExtractGridRows(grid, maxRows, cancellationToken);
            });
        }

        private Task<T> OnUiAsync<T>(Func<T> action)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WinFormsUiContext));
            if (!_form.IsHandleCreated || _form.IsDisposed) throw new InvalidOperationException("The attached form is not available.");
            if (!_form.InvokeRequired) return Task.FromResult(action());

            var tcs = new TaskCompletionSource<T>();
            try
            {
                _form.BeginInvoke((MethodInvoker)delegate
                {
                    try { tcs.TrySetResult(action()); }
                    catch (Exception ex) { tcs.TrySetException(ex); }
                });
            }
            catch (Exception ex) { tcs.TrySetException(ex); }
            return tcs.Task;
        }

        private static Control FindControl(Control root, string id)
        {
            if (root == null) return null;
            if (string.Equals(root.Name, id, StringComparison.OrdinalIgnoreCase)) return root;
            foreach (Control child in root.Controls)
            {
                var found = FindControl(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private static UiControlSnapshot BuildSnapshot(Control control, bool includeChildren)
        {
            var children = new List<UiControlSnapshot>();
            if (includeChildren)
            {
                foreach (Control child in control.Controls)
                    children.Add(BuildSnapshot(child, true));
            }

            var value = ReadValue(control);
            return new UiControlSnapshot
            {
                Id = control.Name,
                Name = control.Name,
                ControlType = control.GetType().FullName,
                Text = control.Text,
                Enabled = control.Enabled,
                Visible = control.Visible,
                Left = control.Left,
                Top = control.Top,
                Width = control.Width,
                Height = control.Height,
                Value = value,
                ValueType = value == null ? null : value.GetType().FullName,
                Children = children.AsReadOnly()
            };
        }

        private static object ReadValue(Control control)
        {
            var textBox = control as TextBoxBase;
            if (textBox != null) return textBox.Text;

            var combo = control as ComboBox;
            if (combo != null) return combo.SelectedItem ?? combo.Text;

            var check = control as CheckBox;
            if (check != null) return check.Checked;

            var radio = control as RadioButton;
            if (radio != null) return radio.Checked;

            var numeric = control as NumericUpDown;
            if (numeric != null) return numeric.Value;

            var date = control as DateTimePicker;
            if (date != null) return date.Value;

            var listBox = control as ListBox;
            if (listBox != null) return listBox.SelectedItem ?? listBox.Text;

            var listView = control as ListView;
            if (listView != null) return listView.SelectedItems.Count == 0 ? null : listView.SelectedItems[0].Text;

            var label = control as Label;
            if (label != null) return label.Text;

            return control.Text;
        }

        private static IReadOnlyList<IReadOnlyDictionary<string, object>> ExtractGridRows(DataGridView grid, int maxRows, CancellationToken cancellationToken)
        {
            var source = GetBoundSource(grid);
            if (source != null)
            {
                var bound = ExtractRowsFromSource(source, maxRows, cancellationToken);
                if (bound.Count > 0 || source is DataTable || source is DataView || source is DataSet)
                    return bound;
            }

            var rows = new List<IReadOnlyDictionary<string, object>>();
            var columns = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToList();
            for (var rowIndex = 0; rowIndex < grid.Rows.Count && rows.Count < maxRows; rowIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = grid.Rows[rowIndex];
                if (row.IsNewRow) continue;
                var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var column in columns)
                    data[column.Name] = row.Cells[column.Index].Value;
                rows.Add(data);
            }
            return rows.AsReadOnly();
        }

        private static object GetBoundSource(DataGridView grid)
        {
            return grid.DataSource;
        }

        private static List<IReadOnlyDictionary<string, object>> ExtractRowsFromSource(object source, int maxRows, CancellationToken cancellationToken)
        {
            var result = new List<IReadOnlyDictionary<string, object>>();
            var table = source as DataTable;
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (result.Count >= maxRows) break;
                    result.Add(RowFromDataRow(row, table.Columns));
                }
                return result;
            }

            var view = source as DataView;
            if (view != null)
            {
                for (var i = 0; i < view.Count && result.Count < maxRows; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    result.Add(RowFromDataRow(view[i].Row, view.Table.Columns));
                }
                return result;
            }

            var enumerable = source as IEnumerable;
            if (enumerable == null || source is string) return result;
            foreach (var item in enumerable)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (result.Count >= maxRows) break;
                result.Add(RowFromObject(item));
            }
            return result;
        }

        private static IReadOnlyDictionary<string, object> RowFromDataRow(DataRow row, DataColumnCollection columns)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn column in columns)
                dict[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
            return dict;
        }

        private static IReadOnlyDictionary<string, object> RowFromObject(object item)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (item == null)
            {
                dict["value"] = null;
                return dict;
            }
            if (item is string || item.GetType().IsPrimitive || item is decimal || item is DateTime)
            {
                dict["value"] = item;
                return dict;
            }
            foreach (var property in item.GetType().GetProperties().Where(p => p.CanRead))
            {
                try { dict[property.Name] = property.GetValue(item, null); }
                catch { }
            }
            return dict;
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
