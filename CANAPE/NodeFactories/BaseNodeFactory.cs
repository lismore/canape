//    CANAPE Network Testing Tool
//    Copyright (C) 2014 Context Information Security
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using CANAPE.DataFrames.Filters;
using CANAPE.Nodes;
using CANAPE.Utils;

namespace CANAPE.NodeFactories
{
    /// <summary>
    /// Base class for pipe node factories
    /// </summary>    
    public abstract class BaseNodeFactory
    {                
        /// <summary>
        /// Id of the node
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Properties of the node
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// List of filters
        /// </summary>
        public IDataFrameFilterFactory[] Filters { get; set; }

        /// <summary>
        /// Whether all the filters need to match (AND) or just one (OR)
        /// </summary>
        public bool MatchAllFilters { get; set; }

        /// <summary>
        /// Whether the node is enable
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The label of the node
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Whether the node should be hidden
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Indicates a selection path, what it means depends on the node
        /// </summary>
        public string SelectionPath { get; set; }

        /// <summary>
        /// Log all input traffic to this node
        /// </summary>
        public bool LogInput { get; set; }

        /// <summary>
        /// Log all output traffic from this node
        /// </summary>
        public bool LogOutput { get; set; }

        /// <summary>
        /// Another internal constructor
        /// </summary>
        /// <param name="label"></param>
        /// <param name="guid"></param>
        protected BaseNodeFactory(string label, Guid guid)
        {
            Label = label;
            Enabled = true;
            Properties = new Dictionary<string, string>();
            Id = guid;
            Filters = new IDataFrameFilterFactory[0];
            SelectionPath = "/";
        }

        /// <summary>
        /// Simple convert to a string
        /// </summary>
        /// <returns>The label of the node</returns>
        public override string ToString()
        {
            return Label;
        }

        /// <summary>
        /// Abstract method to be overridden by derived classes
        /// </summary>
        /// <returns></returns>
        protected abstract BasePipelineNode OnCreate(Logger logger, NetGraph graph, Dictionary<string, object> stateDictionary);

        /// <summary>
        /// Create a new pipeline node based on this factory
        /// </summary>
        /// <param name="graph">The netgraph associated with this node</param>
        /// <param name="logger">The logger associated with the graph</param>
        /// <param name="stateDictionary">A dictionary which can be used to store construction state</param>
        /// <returns>The new node</returns>
        public BasePipelineNode Create(Logger logger, NetGraph graph, Dictionary<string, object> stateDictionary)
        {
            BasePipelineNode node = null;
            try
            {
                node = OnCreate(logger, graph, stateDictionary);
                if (node != null)
                {
                    foreach (var pair in Properties)
                    {
                        node.Properties[pair.Key] = pair.Value;
                    }

                    DataFrameFilterExpression filters = new DataFrameFilterExpression(MatchAllFilters);

                    foreach (IDataFrameFilterFactory factory in Filters)
                    {
                        if (factory.Enabled)
                        {
                            filters.Add(factory.CreateFilter());
                        }
                    }

                    node.Filters = filters;
                    node.Enabled = Enabled;
                    node.SelectionPath = SelectionPath;
                    node.Hidden = Hidden;
                    node.LogInput = LogInput;
                    node.LogOutput = LogOutput;
                }
            }
            catch (Exception e)
            {
                throw new NodeFactoryException(String.Format(CANAPE.Properties.Resources.BaseNodeFactory_Create, this.Label, e.Message), e);
            }

            return node;
        }
    }
}
