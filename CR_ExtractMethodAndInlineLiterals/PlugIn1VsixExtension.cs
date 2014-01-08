using System.ComponentModel.Composition;
using DevExpress.CodeRush.Common;

namespace CR_ExtractMethodAndInlineLiterals
{
    [Export(typeof(IVsixPluginExtension))]
    public class CR_ExtractMethodAndInlineLiteralsExtension : IVsixPluginExtension { }
}