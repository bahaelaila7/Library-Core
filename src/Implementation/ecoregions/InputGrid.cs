using Landis.GeoTiff;
using Landis.Utilities;
using Landis.SpatialModeling;
using Landis.Core;
using System;

namespace Landis.Ecoregions
{
    /// <summary>
    /// An input grid of boolean values based on an ecoregions map.  Active
    /// ecoregions are represented by true values; inactive sites by false
    /// values.
    /// </summary>
    public class InputGrid
        : Grid, IInputGrid<bool>
    {
        private IInputRaster<int> raster;
        private IEcoregionDataset ecoregions;
        private Location pixelLocation;
        private bool disposed = false;

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance using an input raster with ecoregion
        /// pixels.
        /// </summary>
        public InputGrid(IInputRaster<int> raster,
                         IEcoregionDataset            ecoregions)
            : base(raster.Dimensions.Rows,
                   raster.Dimensions.Columns)
        {
            this.raster = raster;
            this.ecoregions = ecoregions;

            // Initialize pixel location so the next call to RowMajor.Next
            // will return upper-left location (1,1)
            this.pixelLocation = new Location(1,0);
        }

        //---------------------------------------------------------------------

        public bool ReadValue()
        {
            if (disposed)
                throw new System.InvalidOperationException("Object has been disposed.");
            raster.ReadBufferPixel();
            int pixel = raster.BufferPixel;
            pixelLocation = RowMajor.Next(pixelLocation, raster.Dimensions.Columns);
            ushort mapCode = (ushort) pixel;
            IEcoregion ecoregion = ecoregions.Find(mapCode);
            //Console.WriteLine("  reading in ecoregion {0} which is {1}", ecoregion.Name, ecoregion.Active);
            if (ecoregion != null)
                return ecoregion.Active;

            string mesg = string.Format("Error at map site {0}", pixelLocation);
            string innerMesg = string.Format("Unknown map code for ecoregion: {0}", mapCode);
            throw new MultiLineException(mesg, innerMesg);
        }

        //---------------------------------------------------------------------

        public void Close()
        {
            Dispose();
        }

        //---------------------------------------------------------------------

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        //---------------------------------------------------------------------

        protected void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    //  Dispose of managed resources.
                    raster.Dispose();
                }
                //  Cleanup unmanaged resources (none).
                disposed = true;
            }
        }

        //---------------------------------------------------------------------

        ~InputGrid()
        {
            Dispose(false);
        }
    }
}
