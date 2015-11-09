<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" version="1.0">
  <xsl:template match="/">
    <xsl:apply-templates select="javasource" />
  </xsl:template>
  <xsl:template match="Field" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <local:VisualElement xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" ToolTip="Field">
      <local:VisualElement.Resources>
        <XmlDataProvider x:Key="sourceData" XPath="/Field" IsInitialLoadEnabled="True" IsAsynchronous="False">
          <x:XData>
            <Field xmlns="">
              <Access>public</Access>
              <Name>field</Name>
              <Type>int</Type>
              <Multiplicity>1</Multiplicity>
            </Field>
          </x:XData>
        </XmlDataProvider>
      </local:VisualElement.Resources>
      <local:JavaField Background="White" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" ToolTip="Multiplicity">
        <local:JavaField.FieldName>
          <xsl:value-of select="Name" />
        </local:JavaField.FieldName>
        <local:JavaField.FieldAccess>
          <xsl:value-of select="Access" />
        </local:JavaField.FieldAccess>
        <local:JavaField.FieldType>
          <xsl:value-of select="Type" />
        </local:JavaField.FieldType>
        <local:JavaField.Multiplicity>
          <xsl:value-of select="Multiplicity" />
        </local:JavaField.Multiplicity>
      </local:JavaField>
    </local:VisualElement>
  </xsl:template>
  <xsl:template match="properties" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <StackPanel Orientation="Vertical" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
      <xsl:apply-templates />
    </StackPanel>
  </xsl:template>
  <xsl:template match="JavaParam" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <local:VisualElement xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" ToolTip="JavaParam">
      <local:VisualElement.Resources>
        <XmlDataProvider x:Key="sourceData" XPath="/JavaParam" IsInitialLoadEnabled="True" IsAsynchronous="False">
          <x:XData>
            <JavaParam xmlns="">
              <Name>ParamN</Name>
              <Type>ParamT</Type>
            </JavaParam>
          </x:XData>
        </XmlDataProvider>
      </local:VisualElement.Resources>
      <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Orientation="Horizontal" Background="White">
        <Label Foreground="Navy" ToolTip="Type">
          <Label.Content>
            <xsl:value-of select="Type" />
          </Label.Content>
        </Label>
        <Label Content=" " />
        <Label ToolTip="Name">
          <Label.Content>
            <xsl:value-of select="Name" />
          </Label.Content>
        </Label>
      </StackPanel>
    </local:VisualElement>
  </xsl:template>
  <xsl:template match="Parameters" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <StackPanel Orientation="Horizontal" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
      <StackPanel.Resources>
        <Style TargetType="UserControl">
          <Setter Property="ContentTemplate">
            <Setter.Value>
              <DataTemplate>
                <StackPanel Orientation="Horizontal">
                  <ContentPresenter />
                  <TextBlock x:Name="commaTB" Text="," xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" />
                </StackPanel>
              </DataTemplate>
            </Setter.Value>
          </Setter>
        </Style>
      </StackPanel.Resources>
      <xsl:apply-templates />
    </StackPanel>
  </xsl:template>
  <xsl:template match="JavaMethod" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <local:VisualElement xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" ToolTip="JavaMethod">
      <local:VisualElement.Resources>
        <XmlDataProvider x:Key="sourceData" XPath="/JavaMethod" IsInitialLoadEnabled="True" IsAsynchronous="False">
          <x:XData>
            <JavaMethod xmlns="">
              <Name>MethodN</Name>
              <ReturnType>int</ReturnType>
              <Access>public</Access>
              <Parameters IsPlaceHolder="true">params</Parameters>
            </JavaMethod>
          </x:XData>
        </XmlDataProvider>
      </local:VisualElement.Resources>
      <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Orientation="Vertical" Background="White" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT">
        <StackPanel Orientation="Horizontal">
          <Label Foreground="Navy" ToolTip="Access">
            <Label.Content>
              <xsl:value-of select="Access" />
            </Label.Content>
          </Label>
          <Label Content=" " />
          <Label Foreground="LightBlue" ToolTip="ReturnType">
            <Label.Content>
              <xsl:value-of select="ReturnType" />
            </Label.Content>
          </Label>
          <Label Content=" " />
          <Label Foreground="Navy" ToolTip="Name">
            <Label.Content>
              <xsl:value-of select="Name" />
            </Label.Content>
          </Label>
          <Label Content="(" />
          <xsl:apply-templates select="Parameters" />
          <Label Content=")" />
        </StackPanel>
        <Label>{</Label>
        <Label>}</Label>
      </StackPanel>
    </local:VisualElement>
  </xsl:template>
  <xsl:template match="methods" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <StackPanel Orientation="Vertical" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
      <xsl:apply-templates />
    </StackPanel>
  </xsl:template>
  <xsl:template match="class-declaration" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <local:VisualElement xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" ToolTip="class-declaration">
      <local:VisualElement.Resources>
        <XmlDataProvider x:Key="sourceData" XPath="/class-declaration" IsInitialLoadEnabled="True" IsAsynchronous="False">
          <x:XData>
            <class-declaration xmlns="">
              <access>public</access>
              <name>className</name>
              <properties IsPlaceHolder="true">properties</properties>
              <methods IsPlaceHolder="true">methods</methods>
            </class-declaration>
          </x:XData>
        </XmlDataProvider>
      </local:VisualElement.Resources>
      <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Orientation="Vertical" Background="White">
        <StackPanel Orientation="Horizontal">
          <Label Foreground="Navy" ToolTip="access">
            <Label.Content>
              <xsl:value-of select="access" />
            </Label.Content>
          </Label>
          <Label Content=" class " Foreground="Navy" />
          <Label Foreground="LightBlue" ToolTip="name">
            <Label.Content>
              <xsl:value-of select="name" />
            </Label.Content>
          </Label>
        </StackPanel>
        <Label>{</Label>
        <xsl:apply-templates select="properties" />
        <Label />
        <xsl:apply-templates select="methods" />
        <Label>}</Label>
      </StackPanel>
    </local:VisualElement>
  </xsl:template>
  <xsl:template match="class-declarations" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <StackPanel Orientation="Vertical" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
      <xsl:apply-templates />
    </StackPanel>
  </xsl:template>
  <xsl:template match="javasource" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <local:VisualElement xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="clr-namespace:CONVErT;assembly=CONVErT" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" ToolTip="javasource">
      <local:VisualElement.Resources>
        <XmlDataProvider x:Key="sourceData" XPath="/javasource" IsInitialLoadEnabled="True" IsAsynchronous="False">
          <x:XData>
            <javasource xmlns="">
              <packageName>packageName</packageName>
              <class-declarations IsPlaceHolder="true">Classes</class-declarations>
            </javasource>
          </x:XData>
        </XmlDataProvider>
      </local:VisualElement.Resources>
      <StackPanel Height="350" Width="350" Orientation="Vertical" Background="White" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
        <Label Content="/* This is automatically generated code */" Foreground="Green" />
        <StackPanel Orientation="Horizontal">
          <Label Content="package " Foreground="Navy" />
          <Label Foreground="LightBlue" ToolTip="packageName">
            <Label.Content>
              <xsl:value-of select="packageName" />
            </Label.Content>
          </Label>
          <Label Content=";" />
        </StackPanel>
        <ScrollViewer Height="300">
          <xsl:apply-templates select="class-declarations" />
        </ScrollViewer>
      </StackPanel>
    </local:VisualElement>
  </xsl:template>
</xsl:stylesheet>