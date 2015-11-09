<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" version="1.0">
  <xsl:template match="/">
    <xsl:apply-templates select="ChartData" />
  </xsl:template>
  <xsl:template match="BarNode" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <local:VisualElement xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" ToolTip="BarNode">
      <local:VisualElement.Resources>
        <XmlDataProvider x:Key="sourceData" XPath="/BarNode" IsInitialLoadEnabled="True" IsAsynchronous="False">
          <x:XData>
            <BarNode xmlns="">
              <Value>70</Value>
              <Name>NewName</Name>
              <Color>Green</Color>
            </BarNode>
          </x:XData>
        </XmlDataProvider>
      </local:VisualElement.Resources>
      <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
        <Label ToolTip="Value">
          <Label.Background>White</Label.Background>
          <Label.Content>
            <xsl:value-of select="Value" />
          </Label.Content>
        </Label>
        <Rectangle ToolTip="Value">
          <Rectangle.Fill>
            <xsl:value-of select="Color" />
          </Rectangle.Fill>
          <Rectangle.Width>10</Rectangle.Width>
          <Rectangle.Height>
            <xsl:value-of select="Value" />
          </Rectangle.Height>
        </Rectangle>
        <Label ToolTip="Name">
          <Label.Background>White</Label.Background>
          <Label.Content>
            <xsl:value-of select="Name" />
          </Label.Content>
        </Label>
      </StackPanel>
    </local:VisualElement>
  </xsl:template>
  <xsl:template match="Bars" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <local:BarChart DockPanel.Dock="Right" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT">
      <xsl:apply-templates />
    </local:BarChart>
  </xsl:template>
  <xsl:template match="ChartData" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <local:VisualElement xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" ToolTip="ChartData">
      <local:VisualElement.Resources>
        <XmlDataProvider x:Key="sourceData" XPath="/ChartData" IsInitialLoadEnabled="True" IsAsynchronous="False">
          <x:XData>
            <ChartData xmlns="">
              <Name>Chart Name</Name>
              <YAxisLabel>YAxis</YAxisLabel>
              <XAxisLabel>XAxis</XAxisLabel>
              <Bars IsPlaceHolder="true">bars</Bars>
            </ChartData>
          </x:XData>
        </XmlDataProvider>
      </local:VisualElement.Resources>
      <DockPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Margin="10" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <TextBlock DockPanel.Dock="Top" TextAlignment="Center" Background="White" ToolTip="Name">
          <TextBlock.Text>
            <xsl:value-of select="Name" />
          </TextBlock.Text>
        </TextBlock>
        <TextBlock DockPanel.Dock="Bottom" TextAlignment="Center" Background="White" ToolTip="XAxisLabel">
          <TextBlock.Text>
            <xsl:value-of select="XAxisLabel" />
          </TextBlock.Text>
        </TextBlock>
        <TextBlock DockPanel.Dock="Left" TextAlignment="Center" Background="White" ToolTip="YAxisLabel">
          <TextBlock.LayoutTransform>
            <RotateTransform Angle="270" />
          </TextBlock.LayoutTransform>
          <TextBlock.Text>
            <xsl:value-of select="YAxisLabel" />
          </TextBlock.Text>
        </TextBlock>
        <xsl:apply-templates select="Bars" />
      </DockPanel>
    </local:VisualElement>
  </xsl:template>
</xsl:stylesheet>