﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsParserCore.Parsers
{
    public class TaskListItem
    {
        public string Description { get; set; }

        public int StartLine { get; set; }

        public int StartColumn { get; set; }
    }
}
