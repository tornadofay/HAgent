using HAgent.WinForms.Helpers.Button.HButtonCore.Rendering;
using HAgent.WinForms.Helpers.Button.HButtonCore.Visuals;
using System.Drawing;

namespace HAgent.WinForms.Helpers.Button.HButtonCore.RenderPass
{
    public interface IRenderPass
    {
        RenderStage Stage { get; }
        void Execute(Graphics g, in HButtonRenderContext ctx, in HButtonLayoutResult layout, in ButtonStyleSnapshot style);
    }
}
