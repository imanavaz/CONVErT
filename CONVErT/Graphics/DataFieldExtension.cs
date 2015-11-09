using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace CONVErT
{
    public class DataFieldExtension : MarkupExtension
    {
        
        public Object Data { get; set; }

        public Type DataType { get; set; }

        public string Address { get; set; }

        public DataFieldExtension() { }

        public DataFieldExtension(string address)
        {
            Address = address;
            Data = null;
        }

        public DataFieldExtension(string address, object data)
        {
            Address = address;
            Data = data;
            DataType = data.GetType();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = serviceProvider.GetService(typeof(IProvideValueTarget))
                as IProvideValueTarget;

            //var host = target.TargetObject as FrameworkElement;
            //if ((ImageSize <= 0 || double.IsNaN(ImageSize)) && host != null)
            //{ ImageSize = host.Width; }

            //string folder = ImageSize > 32 ? "256x256" : "32x32";

            //var source = new BitmapImage(new Uri("pack://application:,,,/MarkupExtensionExample;Component/Images/"+ folder + "/" + SubPath));

            //var prop = target.TargetProperty as DependencyProperty;
            ////If we aren't sure they want a ImageSource, lets give a
            ////full blown image.
            //if (prop == null || prop.PropertyType != typeof(ImageSource))
            //{
            //    var img = new Image();
            //    img.Stretch = Stretch.None;
            //    img.Source = source;
            //    return img;
            //}

            //return source;
            return null;
        }

    }
    
}
