﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace Terminal.Models
{
    public class PluginLoader<T> : IDisposable
    {
        private readonly CompositionContainer _container;

        [ImportMany]
        public IEnumerable<T> Plugins { get; set; }

        public PluginLoader(string path)
        {
            try
            {
                var catalog = new DirectoryCatalog(path);

                this._container = new CompositionContainer(catalog);
                this._container.ComposeParts(this);
            }
            catch
            {
                this.Plugins = new T[0];
            }
        }

        public void Dispose()
        {
            this._container?.Dispose();
        }
    }
}
