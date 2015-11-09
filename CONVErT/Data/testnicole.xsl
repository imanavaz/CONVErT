<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="/">
    <xsl:apply-templates select="Spreadsheet" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
  </xsl:template>
  <xsl:template match="Spreadsheet">
    <HorizontalBarchart>
      <Name>
        <xsl:value-of select="name" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </Name>
      <ScaleName>scale name</ScaleName>
      <HBars>
        <xsl:apply-templates select="sales" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </HBars>
    </HorizontalBarchart>
  </xsl:template>
  <xsl:template match="sales">
    <HorizontalBar>
      <Value>
        <xsl:value-of select="Amount" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </Value>
      <BName>
        <xsl:value-of select="@Region" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </BName>
      <Colour>Red</Colour>
    </HorizontalBar>
  </xsl:template>
</xsl:stylesheet>