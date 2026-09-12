using System;
using System.Collections.Generic;
using HAgent.Abstractions;

namespace HAgent.Models
{
    /// <summary>
    /// Read-only management projection of one authoritative resource's useful content.
    /// </summary>
    public sealed class AiResourceDetail
    {
        public AiResourceInventoryItem InventoryItem { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public IList<AiResourceDetailField> Fields { get; private set; } = new List<AiResourceDetailField>();
        public IList<AiResourceDetailSection> Sections { get; private set; } = new List<AiResourceDetailSection>();

        public void Validate()
        {
            if (InventoryItem == null) throw new ArgumentNullException(nameof(InventoryItem));
            InventoryItem.Validate();
            foreach (var field in Fields ?? new List<AiResourceDetailField>()) field.Validate();
            foreach (var section in Sections ?? new List<AiResourceDetailSection>()) section.Validate();
        }
    }

    public sealed class AiResourceDetailField
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Resource detail field name is required.", nameof(Name));
        }
    }

    public sealed class AiResourceDetailSection
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("Resource detail section title is required.", nameof(Title));
        }
    }
}