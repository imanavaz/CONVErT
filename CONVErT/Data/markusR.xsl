<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="/">
    <xsl:apply-templates select="HorizontalBarchart" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
  </xsl:template>
  <xsl:template match="HorizontalBarchart">
    <Spreadsheet>
      <SpreadSheetName>
        <xsl:value-of select="Name" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </SpreadSheetName>
      <ElementsPresented>Sales</ElementsPresented>
      <Categories>
        <xsl:value-of select="ScaleName" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </Categories>
      <xsl:apply-templates select="HBars/HorizontalBar" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
    </Spreadsheet>
  </xsl:template>
  <xsl:template match="HorizontalBar">
    <sales>
      <xsl:variable name="arg48" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="Value" />
      </xsl:variable>
      <xsl:variable name="output46" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="$arg48 div 2" />
      </xsl:variable>
      <xsl:variable name="output47" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="$arg48 div 2" />
      </xsl:variable>
      <xsl:attribute name="Region" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="BName" />
      </xsl:attribute>
      <Amount1>
        <xsl:copy-of select="$output46" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </Amount1>
      <Amount2>
        <xsl:copy-of select="$output47" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </Amount2>
    </sales>
  </xsl:template>
</xsl:stylesheet>