<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="/">
    <xsl:apply-templates select="Spreadsheet" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
  </xsl:template>
  <xsl:template match="Spreadsheet">
    <HorizontalBarchart>
      <Name>
        <xsl:value-of select="SpreadSheetName" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </Name>
      <ScaleName>
        <xsl:value-of select="Categories" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </ScaleName>
      <HBars>
        <xsl:apply-templates select="sales" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </HBars>
    </HorizontalBarchart>
  </xsl:template>
  <xsl:template match="sales">
    <HorizontalBar>
      <xsl:variable name="arg35" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="Amount1" />
      </xsl:variable>
      <xsl:variable name="arg36" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="Amount2" />
      </xsl:variable>
      <xsl:variable name="output37" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="$arg35 + $arg36 " />
      </xsl:variable>
      <Value>
        <xsl:copy-of select="$output37" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </Value>
      <BName>
        <xsl:value-of select="@Region" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </BName>
      <Colour>Red</Colour>
    </HorizontalBar>
  </xsl:template>
</xsl:stylesheet>