using System;
using System.Collections.Generic;

namespace SitecoreCommander.Agent.Model
{
    internal class RetrieveThePageDetailsResponse : BaseAgentResponse
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public WorkflowInfo Workflow { get; set; } = new();
        public ChildrenInfo Children { get; set; } = new();
        public int Version { get; set; }
        public TemplateInfo Template { get; set; } = new();
        public Dictionary<string, string> Fields { get; set; } = new(); // name-value list
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    internal class WorkflowInfo
    {
        public WorkflowStateInfo WorkflowState { get; set; } = new();
    }

    internal class WorkflowStateInfo
    {
        // Voeg hier properties toe die je verwacht in workflowState
    }

    internal class ChildrenInfo
    {
        public List<object> Property1 { get; set; } = new();
        public List<object> Property2 { get; set; } = new();
        // Pas types aan indien je weet wat er in de arrays zit
    }

    internal class TemplateInfo
    {
        public string TemplateId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
