﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    public interface IMacroCode
    {
        string Name { get; }
        Task RunAsync(IMacroEngine Macro, ModuleManager Modules);
    }
}
