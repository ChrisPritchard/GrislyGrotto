<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:include href="../Shared/Common.xslt"/>

  <xsl:template match="ViewData">

    <script type="text/javascript" src="/Scripts/Blog/Specific.js" />

    <div>
      <h3 class="blogtitle">
        <xsl:value-of select="Blog/@Title" />
      </h3>
      <p class="blogdetail">
        Posted on <xsl:value-of select="Blog/@EntryDate" />
		<xsl:if test="/ViewData/AuthorDetails/Author[@LoggedIn]/@Fullname = Blog/@Author">
          &#160;<a href="/Blog/Editor/{Blog/@BlogID}">Edit</a>
        </xsl:if>
      </p>
      <p class="blogtext">
        <xsl:value-of select="Blog/." disable-output-escaping="yes" />
      </p>
    </div>

    <a name="Comments"/>
    <xsl:for-each select="Comments/Comment">
      <div class="comment">
        <span class="commentdetails">
          Commented by <span class="authorname">
            <xsl:value-of select="@Author" />
          </span>, <xsl:value-of select="@EntryDate" />:
        </span>
        <p class="commenttext">
          <xsl:value-of select="." />
        </p>
        <xsl:if test="/ViewData/AuthorDetails/Author[@LoggedIn]" >
          <form method="post" action="/Blog/DeleteComment">
            <input type="hidden" name="BlogID" value="{/ViewData/Blog/@BlogID}" />
            <input type="hidden" name="CommentID" value="{@CommentID}" />
            <input type="submit" value="Delete Comment" />
          </form>
        </xsl:if>
      </div>
    </xsl:for-each>

    <div>
      <form id="CreateComment" method="post" action="/Blog/CreateComment">
        <input type="hidden" name="BlogID" value="{Blog/@BlogID}" />
        <h3>Add Comment</h3>
        <label for="Author">Author:&#160;</label>
        <input type="text" id="Author" name="Author" />
        <br/>
        <textarea id="Content" name="Content" cols="45" rows="10"></textarea>
        <br/>
        <input type="submit" value="Submit" />
      </form>
    </div>

  </xsl:template>

</xsl:stylesheet>
