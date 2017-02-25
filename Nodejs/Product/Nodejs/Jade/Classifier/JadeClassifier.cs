﻿//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Implements <see cref="IClassifier"/> and provides classification (colorization) of VBScript items
    /// </summary>
    internal sealed class JadeClassifier : TokenBasedClassifier<JadeTokenType, JadeToken>
    {
        public JadeClassifier(ITextBuffer textBuffer, JadeClassifierProvider provider) :
            base(textBuffer, new JadeTokenizer(provider), new JadeClassificationNameProvider(provider.ClassificationRegistryService))
        {
            this.LineBasedClassification = true;
            ServiceManager.AddService<JadeClassifier>(this, textBuffer);
        }

        protected override void RemoveSensitiveTokens(int position, TextRangeCollection<JadeToken> tokens)
        {
            if (tokens.Count > 0)
            {
                var line = this.TextBuffer.CurrentSnapshot.GetLineFromPosition(position);
                var index = tokens.GetFirstItemAfterPosition(line.Start);
                if (index >= 0)
                {
                    for (var i = index; i >= 0; i--)
                    {
                        if (IsAnchorToken(tokens[i].TokenType))
                        {
                            line = this.TextBuffer.CurrentSnapshot.GetLineFromPosition(tokens[i].Start);
                            break;
                        }
                    }
                }

                int start = line.Start;
                var end = tokens[tokens.Count - 1].End;

                if (start < end)
                    tokens.RemoveInRange(TextRange.FromBounds(start, end), true);
            }

            base.RemoveSensitiveTokens(position, tokens);
        }

        protected override int GetAnchorPosition(int position)
        {
            var snapshot = this.TextBuffer.CurrentSnapshot;
            if (position < snapshot.Length)
            {
                var line = snapshot.GetLineFromPosition(position);
                position = line.Start;

                var index = this.Tokens.GetFirstItemAfterPosition(line.Start);
                if (index < 0)
                    index = this.Tokens.Count - 1;

                for (var i = index; i >= 0; i--)
                {
                    if (IsAnchorToken(this.Tokens[i].TokenType))
                    {
                        line = snapshot.GetLineFromPosition(this.Tokens[i].Start);
                        position = line.Start;
                        break;
                    }
                }

                return position;
            }

            return 0;
        }

        private static bool IsAnchorToken(JadeTokenType t)
        {
            switch (t)
            {
                case JadeTokenType.TagKeyword:
                case JadeTokenType.TagName:
                case JadeTokenType.Filter:
                case JadeTokenType.IdLiteral:
                case JadeTokenType.ClassLiteral:
                case JadeTokenType.Comment:
                    return true;
            }

            return false;
        }
    }
}
