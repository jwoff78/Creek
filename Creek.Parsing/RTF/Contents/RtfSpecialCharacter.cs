﻿using Creek.Parsing.RTF.Attributes;
using Creek.Parsing.RTF.Contents.Text;

namespace Creek.Parsing.RTF.Contents
{
    /// <summary>
    /// Represents a page break.
    /// </summary>
    [RtfControlWord("page")]
    public class RtfPageBreak : RtfDocumentContentBase
    {
        
    }

    /// <summary>
    /// Represents a line break.
    /// </summary>
    [RtfControlWord("line")]
    public class RtfLineBreak : RtfParagraphContentBase
    {

    }

    /// <summary>
    /// Represents a tab character.
    /// </summary>
    [RtfControlWord("tab")]
    public class RtfTabCharacter : RtfParagraphContentBase
    {

    }

    /// <summary>
    /// Represents a nonbreaking space.
    /// </summary>
    [RtfControlWord("~")]
    public class RtfNonbreakingSpace : RtfParagraphContentBase
    {

    }

    /// <summary>
    /// Represents an optional hyphen.
    /// </summary>
    [RtfControlWord("-")]
    public class RtfOptionalHyphen : RtfParagraphContentBase
    {

    }
}