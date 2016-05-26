<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" >
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">
    <html xmlns="http://www.w3.org/1999/xhtml" >
      <head runat="server">
        <title>
          <xsl:value-of select="page/@title"/>
        </title>
        <link href="blog/Css.css" type="text/css" rel="stylesheet"/>
      </head>
      <body>
        <form method="post">
          <div class="page">
            <div class="header">
              <h1>The Grisly Grotto</h1>
              <h2>Deviant Minds Think Alike</h2>
            </div>
            <xsl:if test="/page/error/@area = 'session'">
              <h3 class="error">
                Your session has expired. Please hit back, save your content, then relogin and try again.
              </h3>
            </xsl:if>
            <xsl:apply-templates  />
            <div class="footer">
              Site coded and maintained by <a href="mailto:havoc8844@hotmail.com">Christopher Pritchard</a>, 2008. New Zealand Forever
            </div>
          </div>
        </form>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="quote">
    <div xmlns="http://www.w3.org/1999/xhtml">
      <h3>Random Quote:</h3>
      <p class="quoteText">
        <xsl:value-of select="." />
      </p>
      -<xsl:value-of select="@author" />
    </div>
  </xsl:template>

  <xsl:template match="randomimage">
    <div class="randomimage" xmlns="http://www.w3.org/1999/xhtml">
      <img src="{/page/randomimage/@path}"/>
    </div>
  </xsl:template>

  <xsl:template match="userbox">
    <div xmlns="http://www.w3.org/1999/xhtml" class="sidebar">
      <xsl:choose>
        <xsl:when test="/page/loggedauthor">
          <h3>
            Welcome <xsl:value-of select="/page/loggedauthor/@name" />
          </h3>
          <ul type="none">
            <li>
              <input class="button" name="editor" type="submit" value="Post a blog" />
            </li>
            <li>
              <input class="button" name="logout" type="submit" value="Logout" />
            </li>
          </ul>
        </xsl:when>
        <xsl:otherwise>
          <h3>
            Site authors login here
          </h3>
          <ul type="none">
            <li>
              Username:<input class="text" type="text" name="username" />
            </li>
            <li>
              Password:<input class="text" type="password" name="password" />
            </li>
            <xsl:if test="/page/error/@area = 'login'">
              <li class="error">
                Username/Password provided not recognised
              </li>
            </xsl:if>
            <li>
              <input class="button" name="login" type="submit" value="Login" />
            </li>
          </ul>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>

  <xsl:template match="authorlinks">
    <div class="sidebar" xmlns="http://www.w3.org/1999/xhtml">
      <ul class="lined" type="none">
        <xsl:for-each select="authorlink">
          <li class="lined">
            <a class="author sidemenu" href="?author={@name}">
              <xsl:value-of select="@name" />
            </a>
          </li>
        </xsl:for-each>
      </ul>
      <br/>
    </div>
  </xsl:template>

  <xsl:template match="links">
    <div class="sidebar" xmlns="http://www.w3.org/1999/xhtml">
      <span></span>
      <xsl:if test="/page/author/@logged = /page/author/@name">
        <ul type="none">
          <li>
            Title: <input class="text" type="text" name="categorytitle" />
          </li>
          <li>
            <input class="button" type="submit" name="addcategory" value="Add a Link Category" />
          </li>
        </ul>
      </xsl:if>

      <xsl:for-each select="category">
        <h3>
          <xsl:value-of select="@title" />
          <xsl:if test="/page/author/@logged = /page/author/@name">
            <input class="button" type="submit" name="deletecategory" value="Delete" />
            <input type="hidden" name="categorytitle" value="{@title}"/>
          </xsl:if>
        </h3>

        <ul type="none">
          <xsl:if test="/page/author/@logged = /page/author/@name">
            <li>
              Link Text: <input class="text" type="text" name="linktext" />
            </li>
            <li>
              Link Href: <input class="text" type="text" name="linkhref" />
            </li>
            <li>
              <input class="button" type="submit" name="addlink" value="Add a Link" />
              <input type="hidden" name="hiddencategory" value="{@title}" />
            </li>
          </xsl:if>

          <xsl:for-each select="link">
            <li>
              <a class="sidemenu" href="{@href}">
                <xsl:value-of select="@text" />
              </a>
              <xsl:if test="/page/author/@logged = /page/author/@name">
                <input class="button" type="submit" name="deletelink" value="Delete Link" />
                <input type="hidden" name="linktext" value="{@text}"/>
              </xsl:if>
            </li>
          </xsl:for-each>
        </ul>
      </xsl:for-each>
    </div>
  </xsl:template>

  <xsl:template match="recentblogs">
    <div class="sidebar" xmlns="http://www.w3.org/1999/xhtml">
      <h3>
        Recent Posts
      </h3>
      <ul type="none">
        <xsl:for-each select="/page/recentblogs/recentblog">
          <li>
            <a class="sidemenu" href="?author={/page/author/@name}&amp;blog={@title}">
              <xsl:value-of select="@title" />
            </a>
          </li>
        </xsl:for-each>
      </ul>
    </div>
  </xsl:template>

  <xsl:template match="history">
    <div class="sidebar" xmlns="http://www.w3.org/1999/xhtml">
      <h3>
        Blog History
      </h3>
      <ul type="none">
        <xsl:for-each select="/page/history/month">
          <li>
            <a class="sidemenu" href="?author={/page/author/@name}&amp;month={@link}">
              <xsl:value-of select="@text" />
            </a>
          </li>
        </xsl:for-each>
      </ul>
    </div>
  </xsl:template>

  <xsl:template match="blogs">
    <div xmlns="http://www.w3.org/1999/xhtml">
      <h2>
        <xsl:value-of select="/page/author/@name" />'s Blog
      </h2>
      <xsl:for-each select="blog">
        <div>
          <h3>
            <xsl:value-of select="@title" disable-output-escaping="yes" />
          </h3>
          <p>
            <xsl:value-of select="@date" />
            <xsl:if test="@comments">
              <a href="?author={/page/author/@name}&amp;blog={@title}#comments">
                Comments (<xsl:value-of select="@comments" />)
              </a>
            </xsl:if>
            <xsl:if test="/page/author/@logged = /page/author/@name">
              <input type="hidden" name="hiddentitle" value="{@title}" />
              <input class="button" type="submit" name="editor" value="Edit" />
            </xsl:if>
          </p>
          <p class="blogtext">
            <xsl:value-of select="." disable-output-escaping="yes" />
          </p>
        </div>
      </xsl:for-each>
    </div>
  </xsl:template>

  <xsl:template match="comments">
    <div xmlns="http://www.w3.org/1999/xhtml">
      <a name="comments">
        <span></span>
      </a>
      <xsl:if test="comment">
        <h3>Comments</h3>
        <ul type="none">
          <xsl:for-each select="comment">
            <li>
              <h4>
                Posted by <xsl:value-of select="@author" /> on <xsl:value-of select="@date" />
              </h4>
              <p>
                <xsl:value-of select="." />
              </p>
            </li>
          </xsl:for-each>
        </ul>
      </xsl:if>

      <h3>
        Post a comment
      </h3>
      <ul type="none">
        <li>
          Author: <input class="text" type="textbox" name="commentauthor" value="{/page/commenteditor/@author}" />
        </li>
        <xsl:if test="/page/error/@area = 'commentauthor'">
          <li class="error">
            Please specify an author name
          </li>
        </xsl:if>
        <li>
          <textarea class="comment" name="commenttext">
            <xsl:value-of select="/page/commenteditor" />
          </textarea>
        </li>
        <xsl:if test="/page/error/@area = 'commenttext'">
          <li class="error">
            Please enter some text
          </li>
        </xsl:if>
        <li>
          <input class="button" type="submit" name="commentpost" value="Post Comment" />
          <input type="hidden" name="commentblog" value="{/page/blogs/blog/@title}" />
        </li>
      </ul>
    </div>
  </xsl:template>

  <xsl:template match="editor">
    <div>
      <p>
        This editor accepts raw HTML. As such, for each paragraph either add &lt;br/&gt;&lt;br/&gt; to get a line break, or place a &lt;p&gt; before and a &lt;/p&gt; tag after, which has the same effect.
      </p>
      <ul type="none">
        <li>
          Title: <input class="text" type="text" name="blogtitle" value="{/page/editor/blog/@title}" />
        </li>
        <xsl:choose>
          <xsl:when test="/page/error/@area = 'uniquetitle'">
            <li class="error">This title has been used before, sorry</li>
          </xsl:when>
          <xsl:when test="/page/error/@area = 'blogtitle'">
            <li class="error">Please provide a unique title</li>
          </xsl:when>
        </xsl:choose>

        <li>
          <textarea class="editor" name="blogtext">
            <xsl:value-of select="/page/editor/blog" />
          </textarea>
        </li>
        <xsl:if test="/page/error/@area = 'blogtext'">
          <li class="error">Please provide some text</li>
        </xsl:if>

        <li>
          <xsl:choose>
            <xsl:when test="/page/error">
              <input class="button" type="submit" name="blogpost" value="Post" />
            </xsl:when>
            <xsl:when test="/page/editor/blog">
              <input type="hidden" name="hiddentitle" value="{/page/editor/blog/@title}"/>
              <input class="button" type="submit" name="blogpost" value="Update" />
            </xsl:when>
            <xsl:otherwise>
              <input class="button" type="submit" name="blogpost" value="Post" />
            </xsl:otherwise>
          </xsl:choose>
        </li>
        <li>
          <a href="?author={/page/author/@name}">Cancel</a>
        </li>
      </ul>
    </div>
  </xsl:template>

</xsl:stylesheet>
