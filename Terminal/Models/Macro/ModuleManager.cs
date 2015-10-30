﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    public class ModuleManager
    {

        private Dictionary<string, IModule> Dictionary { get; }
        public MacroEngine Engine { get; set; }

        public HashSet<Assembly> Assemblies { get; }
        public HashSet<string> NameSpaces { get; }

        public ModuleManager()
        {
            this.Dictionary = new Dictionary<string, IModule>();
            this.Assemblies = new HashSet<Assembly>();
            this.NameSpaces = new HashSet<string>();
        }

        public T Get<T>() where T : IModule
        {
            var module = this.Dictionary[typeof(T).FullName];
            module.Engine = this.Engine;

            return (T)module;
        }

        public void Register(IModule value)
        {
            var type = value.GetType();

            this.Dictionary[type.FullName] = value;

            var assembly = type.Assembly;
            var space = type.Namespace;

            this.Assemblies.Add(assembly);
            this.NameSpaces.Add(space);

        }

    }
}
