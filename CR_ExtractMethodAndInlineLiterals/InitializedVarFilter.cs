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
    public class InitializedVarFilter : IElementFilter
    {
        private readonly string _name;
        public InitializedVarFilter(string name)
        {
            _name = name;
        }
        public bool Apply(IElement element)
        {
            if (element.ElementType != LanguageElementType.InitializedVariable)
            {
                return false;
            }
            if (element.Name != _name)
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
