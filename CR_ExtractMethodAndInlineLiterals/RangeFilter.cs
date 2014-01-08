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
    public class TypeInRangeFilter : IElementFilter
    {
        private readonly SourceRange _selectRange;
        private readonly LanguageElementType _languageElementType;
        public TypeInRangeFilter(LanguageElementType languageElementType, SourceRange selectRange)
        {
            _languageElementType = languageElementType;
            _selectRange = selectRange;
        }

        public bool Apply(IElement element)
        {
            if (!_selectRange.Contains(element.FirstRange.Start))
            {
                return false;
            }
            if (element.ElementType != _languageElementType)
            {
                return false;
            }
            return true;
        }

        public bool SkipChildren(IElement element)
        {
            return false;
        }
    }
}
