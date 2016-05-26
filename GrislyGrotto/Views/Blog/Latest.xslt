<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:include href="../Shared/Common.xslt"/>

  <xsl:template match="ViewData">

    <xsl:for-each select="Blogs/Blog">
      <div class="blog">
        <h3 class="blogtitle">
          <xsl:value-of select="@Title" />
        </h3>
        <p class="blogdetail">
          <xsl:if test="not(/ViewData/AuthorDetails/Author[@Current])">
            Posted by <span class="authorname">
              <xsl:value-of select="@Author" />
            </span>,&#160;
          </xsl:if>
          <xsl:value-of select="@EntryDate" />&#160;
          <xsl:if test="/ViewData/AuthorDetails/Author[@LoggedIn]/@Fullname = @Author">
            <a href="/Blog/Editor/{@BlogID}">Edit</a>&#160;
          </xsl:if>
          <a href="/Blog/Specific/{@BlogID}#Comments">
            Comments (<xsl:value-of select="@Comments"/>)
          </a>
        </p>
        <p class="blogtext">
          <xsl:value-of select="." disable-output-escaping="yes" />
        </p>
      </div>
    </xsl:for-each>

  </xsl:template>

</xsl:stylesheet>
