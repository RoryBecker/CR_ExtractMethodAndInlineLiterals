using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.CodeRush.Core;
using DevExpress.CodeRush.PlugInCore;
using DevExpress.CodeRush.StructuralParser;
using DevExpress.Refactor;

namespace CR_ExtractMethodAndInlineLiterals
{
    public partial class PlugIn1 : StandardPlugIn
    {
        // DXCore-generated code...
        #region InitializePlugIn
        public override void InitializePlugIn()
        {
            base.InitializePlugIn();
            registerExtractMethodAndInlineLiterals();
        }
        #endregion
        #region FinalizePlugIn
        public override void FinalizePlugIn()
        {
            //
            // TODO: Add your finalization code here.
            //

            base.FinalizePlugIn();
        }
        #endregion

        private IList<string> ToInline;
        public void registerExtractMethodAndInlineLiterals()
        {
            DevExpress.Refactor.Core.RefactoringProvider ExtractMethodAndInlineLiterals = new DevExpress.Refactor.Core.RefactoringProvider(components);
            ((System.ComponentModel.ISupportInitialize)(ExtractMethodAndInlineLiterals)).BeginInit();
            ExtractMethodAndInlineLiterals.ProviderName = "ExtractMethodAndInlineLiterals"; // Should be Unique
            ExtractMethodAndInlineLiterals.DisplayName = "Extract Method and Inline Literals";
            ExtractMethodAndInlineLiterals.CheckAvailability += ExtractMethodAndInlineLiterals_CheckAvailability;
            ExtractMethodAndInlineLiterals.Apply += ExtractMethodAndInlineLiterals_Execute;
            ((System.ComponentModel.ISupportInitialize)(ExtractMethodAndInlineLiterals)).EndInit();
        }
        private void ExtractMethodAndInlineLiterals_CheckAvailability(Object sender, CheckContentAvailabilityEventArgs ea)
        {
            RefactoringProviderBase ExtractMethod = CodeRush.Refactoring.Get("Extract Method");
            ea.Available = ExtractMethod.GetAvailability() == RefactoringAvailability.Available;
        }
        private void ExtractMethodAndInlineLiterals_Execute(Object sender, ApplyContentEventArgs ea)
        {

            TextDocument ActiveDoc = CodeRush.Documents.ActiveTextDocument;
            using (ActiveDoc.NewCompoundAction("Extract Method and Inline Literals"))
            {
                // Initialization
                ToInline = new List<string>();

                // Get Selection
                SourceRange SelectRange = GetBoundRangeOfSelection();

                ExtractPrimitivesAsLocals(ActiveDoc, SelectRange);
                MoveLocalInitializationsAboveSelection(ActiveDoc, SelectRange);
                ExtractRemainingSelection(ActiveDoc, SelectRange);
                InlineTempVariables(ActiveDoc);
                LineUpArgumentsOfFinalMethodCall(ActiveDoc);
            }
        }
        private void ExtractPrimitivesAsLocals(TextDocument ActiveDoc, SourceRange SelectRange)
        {
            // Extract Primitives as Locals
            var PrimitiveObjects = (from item in GetPrimitives(SelectRange).OfType<PrimitiveExpression>() select item).ToList();

            int N = PrimitiveObjects.Count();
            for (int i = PrimitiveObjects.Count - 1; i >= 0; i--)
            {
                N -= 1;
                Select(PrimitiveObjects[i].Range);
                ExecuteRefactoring("Introduce Local");
                SetVarName("Param" + N);
                ToInline.Add("Param" + N);
            }
            ActiveDoc.ParseIfTextChanged();
        }
        private void MoveLocalInitializationsAboveSelection(TextDocument ActiveDoc, SourceRange SelectRange)
        {
            // Move Local Initializations above Selection
            for (int j = ToInline.Count - 1; j >= 0; j--)
            {
                var InitializedVar = GetInitializedVars(ToInline[j]).FirstOrDefault();
                InitializedVar.MoveTo(SelectRange.Top, "MoveAssignment" + j);
                ActiveDoc.ParseIfTextChanged();
            }
        }
        private static void ExtractRemainingSelection(TextDocument ActiveDoc, SourceRange SelectRange)
        {
            // Extract Remaining Selection
            CodeRush.Selection.SelectRange(SelectRange);
            ExecuteRefactoring("Extract Method");
            ActiveDoc.ParseIfTextChanged();
        }
        private void InlineTempVariables(TextDocument ActiveDoc)
        {
            // Inline Temp Variables
            for (int j = ToInline.Count - 1; j >= 0; j--)
            {
                var InitializedVar = GetInitializedVars(ToInline[j]).FirstOrDefault();
                Select(InitializedVar.NameRange);
                // Inline literal
                ExecuteRefactoring("Inline Temp");
                ActiveDoc.ParseIfTextChanged();
            }
        }
        private static void LineUpArgumentsOfFinalMethodCall(TextDocument ActiveDoc)
        {
            // Line up arguments of final method call.
            SourceRange FirstPrimRange = SourceRange.Empty;
            try
            {
                SourceRange LineRange = ActiveDoc.GetLineRange(CodeRush.Caret.Line);
                FirstPrimRange = GetPrimitives(LineRange).OfType<PrimitiveExpression>().First().Range;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            CodeRush.Caret.MoveTo(FirstPrimRange.Start);
            var Node = DevExpress.CodeRush.Core.CodeRush.Documents.ActiveTextDocument.GetNodeAt(DevExpress.CodeRush.Core.CodeRush.Caret.SourcePoint);
            ExecuteRefactoring("Line-up Parameters");
            CodeRush.Documents.ActiveTextDocument.ParseIfTextChanged();
        }
        private void SetVarName(string Name)
        {
            CodeRush.Documents.ActiveTextDocument.SetText(GetSelectionRange(), Name);
        }

        private static SourceRange GetBoundRangeOfSelection()
        {
            SourceRange SelectRange = GetSelectionRange();
            SelectRange.BindToCode(CodeRush.Documents.ActiveTextDocument);
            return SelectRange;
        }
        private static SourceRange GetSelectionRange()
        {
            SourceRange SelectRange = SourceRange.Empty;
            CodeRush.Documents.ActiveTextDocument.GetSelection(out SelectRange);
            return SelectRange;
        }
        private void Select(SourceRange range)
        {
            CodeRush.Selection.SelectRange(range);
        }
        private static void SetCaretWithinLE(SourceRange range)
        {
            CodeRush.Caret.MoveTo(range.Start.OffsetPoint(0, 1));
        }
        private static void ExecuteRefactoring(string RefactoringName)
        {
            var Refactoring = CodeRush.Refactoring.Get(RefactoringName);
            var Refactorings = CodeRush.Refactoring.Providers.Where((P) => { return P.IsAvailable; });
            CodeRush.SmartTags.UpdateContext();
            if (Refactoring.IsAvailable)
            {
                Refactoring.IsNestedProvider = true;
                Refactoring.Execute();
            }
        }
        private static IEnumerable<InitializedVariable> GetInitializedVars(string literalName)
        {
            var InitializedVars = new ElementEnumerable(CodeRush.Source.ActiveFileNode,
                            new InitializedVarFilter(literalName), true);
            return InitializedVars.OfType<InitializedVariable>();
        }
        private static ElementEnumerable GetPrimitives(SourceRange SelectRange)
        {
            // Find all literals in SelectRange.
            var RangePrimitives = new ElementEnumerable(CodeRush.Source.ActiveFileNode,
                new TypeInRangeFilter(LanguageElementType.PrimitiveExpression,
                  SelectRange), true);
            return RangePrimitives;
        }
    }
}