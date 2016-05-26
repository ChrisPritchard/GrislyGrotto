<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">

    <rss version="2.0">
      <channel>
        <title>The Grisly Grotto</title>
        <link>http://grislygrotto.co.nz/</link>
        <description>Grisly Grotto - Deviant Minds Think Alike</description>
        <language>en</language>
        <xsl:for-each select="//Post">
          <item>
            <title>
              <xsl:value-of select="Title"/>
            </title>
            <author>
              <xsl:value-of select="Author"/>
            </author>
            <link>
              http://grislygrotto.co.nz/?id=<xsl:value-of select="Id"/>
            </link>
            <description>
              <xsl:value-of select="Content" />
            </description>
            <pubDate>
              <xsl:value-of select="TimePosted"/>
            </pubDate>
            <guid>
              http://grislygrotto.co.nz/?id=<xsl:value-of select="Id"/>
            </guid>
          </item>
        </xsl:for-each>
      </channel>
    </rss>

  </xsl:template>
</xsl:stylesheet>
