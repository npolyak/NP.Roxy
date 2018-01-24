// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author(s) of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NP.Roxy
{
    internal class RoslynWorkspaceBase : Workspace
    {
        public RoslynWorkspaceBase(HostServices host, string workspaceKind = "Custom") :
                base(host, workspaceKind)
        {

        }

        public override bool CanApplyChange(ApplyChangesKind feature) =>
            true;

        public override bool CanOpenDocuments => false;

        public new void ClearSolution()
        {
            base.ClearSolution();
        }

        public ProjectId CurrentProjectId { get; private set; }


        /// <summary>
        /// Adds an entire solution to the workspace, replacing any existing solution.
        /// </summary>
        public Solution AddSolution(SolutionInfo solutionInfo)
        {
            if (solutionInfo == null)
            {
                throw new ArgumentNullException(nameof(solutionInfo));
            }

            this.OnSolutionAdded(solutionInfo);

            this.UpdateReferencesAfterAdd();

            return this.CurrentSolution;
        }


        /// <summary>
        /// Adds a project to the workspace. All previous projects remain intact.
        /// </summary>
        public Project AddProject(string name, string language)
        {
            ProjectId projectId = ProjectId.CreateNewId();
            var info = ProjectInfo.Create(projectId, VersionStamp.Create(), name, name, language);

            CurrentProjectId = projectId;

            Project result = this.AddProject(info);

            return result;
        }

        public Project CurrentProj =>
            this.CurrentSolution.GetProject(CurrentProjectId);

        /// <summary>
        /// Adds a project to the workspace. All previous projects remain intact.
        /// </summary>
        public Project AddProject(ProjectInfo projectInfo)
        {
            if (projectInfo == null)
            {
                throw new ArgumentNullException(nameof(projectInfo));
            }

            CurrentProjectId = projectInfo.Id;

            this.OnProjectAdded(projectInfo);

            this.UpdateReferencesAfterAdd();

            return this.CurrentSolution.GetProject(projectInfo.Id);
        }


        /// <summary>
        /// Adds a document to the workspace.
        /// </summary>
        public Document AddDocument(string name, SourceText text, ProjectId projectId = null)
        {
            if (projectId == null)
            {
                projectId = CurrentProjectId;

                if (projectId == null)
                {
                    throw new ArgumentNullException(nameof(projectId));
                }
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var documentId = DocumentId.CreateNewId(projectId);
            var loader = TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create()));

            return this.AddDocument(DocumentInfo.Create(documentId, name, loader: loader));
        }

        public Document AddDocument(string name, string text, ProjectId projectId = null)
        {
            SourceText sourceText = SourceText.From(text);

            return AddDocument(name, sourceText, projectId);
        }


        /// <summary>
        /// Adds a document to the workspace.
        /// </summary>
        private Document AddDocument(DocumentInfo documentInfo)
        {
            if (documentInfo == null)
            {
                throw new ArgumentNullException(nameof(documentInfo));
            }

            this.OnDocumentAdded(documentInfo);

            return this.CurrentSolution.GetDocument(documentInfo.Id);
        }
    }
}
