using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CONVErT
{
    public class BarChart : Canvas
    {

        public BarChart()
        {
            this.Loaded += new RoutedEventHandler(OnLoad);
            this.Background = Brushes.White;

        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            
            //get bars
            UIElement[] bars = new UIElement [this.Children.Count];

            
            //need this part for normalaising height of bars int he chart
            int maxaxisheight = 0;
            int chartWidth = 30;
            int BarCount = 0;
            foreach (UIElement o in this.Children)
            {
                if ((o is VisualElement))
                {
                    BarCount++;

                    int elementHeight = (int)Math.Ceiling((o as VisualElement).ActualHeight);
                    int elementWidth = (int)Math.Ceiling((o as VisualElement).ActualWidth);
                    
                    if (elementHeight > maxaxisheight)
                        maxaxisheight = elementHeight;

                    chartWidth += elementWidth;
                }
            }

            int barLabelGap = (BarCount == 0 ? 5 : 25);
            
            int offeredHeight = 250;//default
            if (maxaxisheight != 0)
            {
                offeredHeight = maxaxisheight + barLabelGap + 5;
            }

            //generate axis
            chartWidth = (BarCount == 0 ? 50 : chartWidth);
            generateChartArea(offeredHeight, chartWidth, BarCount);
            
            if (BarCount != 0) //We have bars
            {
                //put bars in 
                int horizontalPlaceholder = 20;
                for (int i = 0; i < this.Children.Count; i++)
                {
                    if ((this.Children[i] is VisualElement))
                    {
                        //prepare view box for large items
                        Canvas.SetLeft(this.Children[i], (horizontalPlaceholder));
                        Canvas.SetTop(this.Children[i], offeredHeight - 8 - (this.Children[i] as VisualElement).ActualHeight);
                        this.Children[i].IsHitTestVisible = true;
                        
                        horizontalPlaceholder += (int)Math.Ceiling((this.Children[i] as VisualElement).ActualWidth);
                    }
                }
            }


        }

        private int normalizeSize(int offeredHeight, double p)
        {
            return (int)Math.Ceiling((offeredHeight / p) * p);
        }

        private void generateChartArea(int h, int w , int barcount)
        {
            int width = w;
            this.Width = width;
            this.Height = h;

            int barLabelGap = (barcount == 0 ? 5 : 25);
            
            //create axes
            //X
            ArrowLine xaxis = new ArrowLine();
            xaxis.Stroke = Brushes.Black;
            xaxis.StrokeThickness = 2;
            xaxis.IsArrowClosed = true;
            xaxis.ArrowLength = 4;

            xaxis.X1 = 5;
            xaxis.Y1 = Height - barLabelGap - 5;
            xaxis.X2 = width - 5;
            xaxis.Y2 = xaxis.Y1;
            Canvas.SetLeft(xaxis, 0);
            Canvas.SetTop(xaxis, 0);
            this.Children.Add(xaxis);
            
            //Y
            ArrowLine yaxis = new ArrowLine();
            yaxis.Stroke = Brushes.Black;
            yaxis.StrokeThickness = 2;
            yaxis.IsArrowClosed = true;
            yaxis.ArrowLength = 4;

            yaxis.X1 = 10;
            yaxis.Y1 = h - barLabelGap;
            yaxis.X2 = yaxis.X1;
            yaxis.Y2 = 5;

            Canvas.SetLeft(yaxis, 0);
            Canvas.SetTop(yaxis, 0);
            this.Children.Add(yaxis);
        }

    }
}

/*private VisualElement renderBarchart(XmlNode xNode)
        {
            XmlNode bars = xNode.SelectSingleNode("Bars");

            //prepare chart area=============

            Canvas c = new Canvas();
            c.Background = Brushes.White;
            c.Height = 285;
            c.Width = 40 + 5 + bars.ChildNodes.Count * 20;

            //get chart name
            string chartName = xNode.SelectNodes("name")[0].InnerText;
            Label chartNameLable = new Label();
            //chartNameLable.Height = 15;
            chartNameLable.FontSize = 8;
            chartNameLable.Content = chartName;

            Canvas.SetTop(chartNameLable, 0);
            Canvas.SetLeft(chartNameLable, 30);
            c.Children.Add(chartNameLable);

            //prepare axis
            // Draw X axis
            ArrowLine xaxis = new ArrowLine();
            xaxis.Stroke = Brushes.Black;
            xaxis.StrokeThickness = 2;
            xaxis.IsArrowClosed = true;
            xaxis.ArrowLength = 4;

            xaxis.X1 = 5;
            xaxis.Y1 = 225;
            xaxis.X2 = 30 + bars.ChildNodes.Count * 20;
            xaxis.Y2 = 225;

            Canvas.SetLeft(xaxis, 0);
            Canvas.SetTop(xaxis, 0);
            c.Children.Add(xaxis);

            string xlabel = xNode.SelectNodes("xaxis")[0].InnerText;

            Label txlabel = new Label();
            txlabel.Content = xlabel;
            //txlabel. = TextAlignment.Center;

            Canvas.SetLeft(txlabel, xaxis.X2 / 2 - 20);
            Canvas.SetTop(txlabel, 260);

            c.Children.Add(txlabel);

            // DrawY axis
            string ylabel = xNode.SelectNodes("yaxis")[0].InnerText;

            TextBlock tylabel = new TextBlock();
            tylabel.Text = ylabel;
            tylabel.TextAlignment = TextAlignment.Center;
            tylabel.LayoutTransform = new RotateTransform(90);

            Canvas.SetLeft(tylabel, 0);
            Canvas.SetTop(tylabel, 100);

            c.Children.Add(tylabel);

            ArrowLine yaxis = new ArrowLine();
            yaxis.Stroke = Brushes.Black;
            yaxis.StrokeThickness = 2;
            yaxis.IsArrowClosed = true;
            yaxis.ArrowLength = 4;

            yaxis.X1 = 20;
            yaxis.Y1 = 235;
            yaxis.X2 = 20;
            yaxis.Y2 = 25;

            Canvas.SetLeft(yaxis, 0);
            Canvas.SetTop(yaxis, 0);
            c.Children.Add(yaxis);
            //=============

            //prepare bars 

            VisualElement[] barElements = new VisualElement[bars.ChildNodes.Count];

            //get maximum value for y axis
            //double maximumValue = 0;

            for (int i = 0; i < bars.ChildNodes.Count; i++)
            {
                XmlNode bar = bars.ChildNodes.Item(i);
                double temp = 0;
                if (bar.SelectNodes("value").Count > 0)
                    temp = Convert.ToDouble(bar.SelectNodes("value")[0].InnerText);
                if (temp > maximumValue)
                    maximumValue = temp;
            }

            for (int i = 0; i < bars.ChildNodes.Count; i++)
            {

                XmlNode bar = bars.ChildNodes.Item(i);

                VisualElement temp = render(bar) as VisualElement;

                if (temp != null)
                {
                    barElements[i] = temp;
                    Canvas.SetLeft(barElements[i], (10 + 2 + i * 20));
                    Canvas.SetTop(barElements[i], 225 - elementHeight);
                    barElements[i].IsHitTestVisible = true;

                    c.Children.Add(barElements[i]);
                }
            }


            //prepare chart element
            XmlNode tempChartData = xNode;
            XmlNode tempNode = tempChartData.SelectSingleNode("Bars");
            tempNode.RemoveAll();
            tempNode.InnerText = "Bars";

            VisualElement chart = new VisualElement();
            chart.Data = tempChartData;
            chart.Content = c;

            Canvas.SetLeft(chart, canvasLayingPosition.X);
            Canvas.SetTop(chart, canvasLayingPosition.Y);

            return chart;
        }*/