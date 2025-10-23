using System.Collections.Generic;

namespace BpmnEditor.Models
{
    public class BpmnElement
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string ServiceClass { get; set; }
        public List<BpmnParameter> InputParameters { get; set; } = [];
        public List<BpmnParameter> OutputParameters { get; set; } = [];
    }

    public class BpmnParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class ToolItem
    {
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Label { get; set; }
        public string Category { get; set; }
    }
}