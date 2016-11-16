using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
                //var catalog = new DirectoryCatalog(path);
                var catalog = new SafeDirectoryCatalog(path);

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

    /// <summary>
    /// http://stackoverflow.com/questions/4144683/handle-reflectiontypeloadexception-during-mef-composition
    /// </summary>
    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
        private readonly AggregateCatalog _catalog;

        public SafeDirectoryCatalog(string directory)
        {
            var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);

            _catalog = new AggregateCatalog();

            foreach (var file in files)
            {
                try
                {
                    var asmCat = new AssemblyCatalog(file);

                    //Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (asmCat.Parts.ToList().Count > 0)
                        _catalog.Catalogs.Add(asmCat);
                }
                catch (ReflectionTypeLoadException e)
                {
                    Debug.WriteLine(e.ToString());
                    var ie = e.LoaderExceptions.FirstOrDefault();
                    if (ie != null)
                    {
                        Debug.WriteLine(ie.ToString());
                    }
                }
                catch (BadImageFormatException e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
        }
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return _catalog.Parts; }
        }
    }
}
