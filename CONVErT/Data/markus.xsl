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
      <xsl:variable name="arg46" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="Amount1" />
      </xsl:variable>
      <xsl:variable name="arg47" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="Amount2" />
      </xsl:variable>
      <xsl:variable name="output48" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:value-of select="$arg46 + $arg47 " />
      </xsl:variable>
      <Value>
        <xsl:copy-of select="$output48" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </Value>
      <BName>
        <xsl:value-of select="@Region" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" />
      </BName>
      <Colour>
        <xsl:choose xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:when test="Amount1 = Amount2 ">Green</xsl:when>
          <xsl:when test="Amount1 &gt; Amount2 ">Red</xsl:when>
          <xsl:otherwise>Yellow</xsl:otherwise>
        </xsl:choose>
      </Colour>
    </HorizontalBar>
  </xsl:template>
</xsl:stylesheet>