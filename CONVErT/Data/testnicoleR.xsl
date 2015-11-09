<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="/">
    <xsl:apply-templates select="HorizontalBarchart" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
  </xsl:template>
  <xsl:template match="HorizontalBarchart">
    <Spreadsheet>
      <name>
        <xsl:value-of select="Name" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </name>
      <xsl:apply-templates select="HBars/HorizontalBar" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
    </Spreadsheet>
  </xsl:template>
  <xsl:template match="HorizontalBar">
    <sales>
      <xsl:attribute name="Region" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="BName" />
      </xsl:attribute>
      <Amount>
        <xsl:value-of select="Value" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </Amount>
    </sales>
  </xsl:template>
</xsl:stylesheet>