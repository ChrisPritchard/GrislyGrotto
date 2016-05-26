<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="/">

        <feed xmlns="http://www.w3.org/2005/Atom" xml:lang="en">
            <title>grislygrotto.co.nz</title>
            <link href="http://grislygrotto.co.nz/" rel="alternate"></link>
            <id>http://grislygrotto.co.nz/</id>
            <updated>
                <xsl:value-of select="ViewData/Date/."/>
            </updated>
            <xsl:for-each select="ViewData/Blogs/Blog">
                <entry>
                    <title>
                        <xsl:value-of select="@Title"/>
                    </title>
                    <author>
                        <xsl:value-of select="@Author"/>
                    </author>
                    <link href="http://grislygrotto.co.nz/Blog/Specific/{@BlogID}" rel="alternate"></link>
                    <updated>
                        <xsl:value-of select="@EntryDate"/>
                    </updated>
                    <id>
                        http://grislygrotto.co.nz/Blog/Specific/<xsl:value-of select="@BlogID"/>
                    </id>
                    <summary type="html">
                        <xsl:value-of select="." />
                    </summary>
                </entry>
            </xsl:for-each>
        </feed>

    </xsl:template>
</xsl:stylesheet>
