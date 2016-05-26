<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="html" indent="yes"/>
    <xsl:template match="/">

      <html>
        <head>
          <title>Grisly Grotto - Deviant Minds Think Alike</title>
          <link rel="alternate" type="application/rss+xml" title="RSS 2.0" href="/Format/RSS" />
          <link href="/Resources/Site.css" type="text/css" rel="Stylesheet" />
          <meta http-equiv="expires" content="0" />
          <meta http-equiv="cache-control" content="no-cache" />
          <meta http-equiv="pragma" content="no-cache" />
        </head>
        <body>
          
          <div class="header">
            <h1>The Grisly Grotto</h1>
            <h2>Deviant Minds Think Alike</h2>
          </div>

          <div class="userLinks">
            <a href="/Christopher" class="userLink">Christopher</a>
            <span class="seperator">|</span>
            <a href="/Peter" class="userLink">Peter</a>
            <span class="seperator">|</span>
            <a href="/Ben" class="userLink">Ben</a>
            <span class="seperator">|</span>
            <a href="/Kelly" class="userLink">Kelly</a>
          </div>
          
          <div class="loginBox">
            <xsl:choose>
              <xsl:when test="Page/@LoggedUser">
                <h3>
                  Welcome <xsl:value-of select="Page/@LoggedUser"/>
                </h3>
                <p>
                  <a href="/Editor/New">Add new post</a>
                  <br/>
                  <a href="/Action/Logout">Logout</a>
                </p>
                <xsl:if test="Page/Drafts/Post">
                <h4>Drafts</h4>
                  <ul>
                    <xsl:for-each select="Page/Drafts/Post">
                      <li>
                        <a href="/Posts/{@ID}">
                          <xsl:value-of select="@Title"/>
                        </a>
                      </li>
                    </xsl:for-each>
                  </ul>
                </xsl:if>
              </xsl:when>
              <xsl:otherwise>
                <form action="/Action/Login">
                  <table border="0" align="right">
                    <tr>
                      <td>Username:</td>
                      <td>
                        <input type="text" id="loginName" name="LoginName" />
                      </td>
                    </tr>
                    <tr>
                      <td>Password:</td>
                      <td>
                        <input type="password" id="loginPassword" name="LoginPassword" />
                      </td>
                    </tr>
                    <tr>
                      <td colspan="2">
                        <input type="checkbox" name="RememberMe" />&#160;Remember Me
                      </td>
                    </tr>
                    <tr class="error">
                      <td colspan="2" id="loginError"></td>
                    </tr>
                    <tr>
                      <td colspan="2" align="right">
                        <input type="submit" value="Login" onclick="return checkLogin()" />
                      </td>
                    </tr>
                  </table>
                </form>
              </xsl:otherwise>
            </xsl:choose>
          </div>

          <div class="recentPosts">
            <span class="historyTitle">Recent Posts</span>
            <ul>
              <xsl:for-each select="Page/RecentPosts/Post">
                <li>
                  <a href="/{@User}/Posts/{@ID}">
                    <span class="recentTitle">
                      <xsl:value-of select="@Title"/>
                    </span>
                    <span class="howRecent">
                      <xsl:value-of select="@Created"/>
                    </span>
                  </a>
                </li>
              </xsl:for-each>
            </ul>
          </div>

          <div class="searchBox">
            <form id="searchForm">
              Search: <input type="text" id="searchTerm" name="SearchTerm" value="{Page/@SearchTerm}" /><input type="submit" value="Go" onclick="setSearchTerm()" />
            </form>
          </div>

          <div class="postHistory">
            <span class="historyTitle">Post History</span>
            <ul>
              <xsl:for-each select="Page/PostHistory/MonthPostCount">
                <li>
                  <a href="/Posts/{@Month}/{@Year}">
                    <xsl:value-of select="@Month"/>, <xsl:value-of select="@Year"/> (<xsl:value-of select="@Count"/>)
                  </a>
                </li>
              </xsl:for-each>
            </ul>
          </div>

          <div class="sideMenuBase" />

          <xsl:if test="Page/Quote">
            <div class="quote">
              <span>
                <xsl:value-of select="Page/Quote/."/>
              </span>
              <h4>
                <xsl:value-of select="Page/Quote/@Author"/>
              </h4>
            </div>
          </xsl:if>

          <xsl:if test="not(Page/Editor)">
            
            <div class="posts">
              <xsl:if test="not(Page/Post)">
                <h2>No Posts Found</h2>
                <a href="/">Back to home page...</a>
              </xsl:if>
              <xsl:for-each select="Page/Post">
                <div class="post">
                  <span class="postTitle" id="postTitle{@ID}">
                    <xsl:value-of select="@Title"/>
                  </span>
                  <h4>
                    <xsl:value-of select="@User"/>
                  </h4>
                  <h5>
                    <xsl:value-of select="@Created"/>
                  </h5>
                  <div id="postContent{@ID}">
                    <xsl:value-of select="." disable-output-escaping="yes"/>
                  </div>
                  <div>
                    <xsl:if test="count(/Page/Post) > 1">
                      <a href="/Posts/{@ID}" class="commentsLink">
                        Comments (<xsl:value-of select="@CommentCount"/>)
                      </a>
                    </xsl:if>
                    <xsl:if test="/Page/@LoggedUser = @User">
                      <a href="/Editor/{@ID}" class="editLink">Edit</a>
                    </xsl:if>
                  </div>
                </div>
              </xsl:for-each>
            </div>

            <xsl:if test="count(/Page/Post) = 1">
              <div class="comments">
                <xsl:for-each select="Page/Comment">
                  <div class="comment">
                    <h4>Commented by <xsl:value-of select="@Author"/>, <xsl:value-of select="@Created"/></h4>
                    <p>
                      <xsl:value-of select="."/>
                    </p>
                  </div>
                </xsl:for-each>
              </div>
              <form action="/Action/Comment">
                <input type="hidden" name="PostID" value="{Page/Post/@ID}" />
                <h3>
                  Author: <input type="text" id="commentAuthor" name="Author" />
                </h3>
                <p>
                  <textarea id="commentContent" name="Content" rows="5" cols="40" />
                </p>
                <span id="commentError" class="error" />
                <br/>
                <input type="submit" value="Add Comment" onclick="return checkCommentFields()" />
              </form>
            </xsl:if>
            
          </xsl:if>

          <xsl:if test="Page/Editor">
            <div class="editor">
              <form action="/Action/Post" method="post">
                <input type="hidden" id="editorPostID" name="PostID" value="{Page/Editor/Post/@ID}" />
                <input type="hidden" id="loggedUser" name="LoggedUser" value="{Page/@LoggedUser}" />
                <h3>
                  Title: <input type="text" id="editorTitle" name="Title" value="{Page/Editor/Post/@Title}" />
                </h3>
                <p>
                  <textarea class="ckeditor"  id="editorContent" name="Content" rows="15" cols="80">
                    <xsl:value-of select="Page/Editor/Post/."/>
                  </textarea>
                </p>
                <span id="editorError" class="error" />
                <br/>
                <input type="submit" value="Submit" onclick="return checkPostFields()" />
              </form>
            </div>
          </xsl:if>

          <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js" type="text/javascript" />
          <script src="/Resources/NicEditor/NicEdit.js" type="text/javascript" />
          <script src="/Resources/Site.js" type="text/javascript" />
        </body>        
      </html>
      
    </xsl:template>
</xsl:stylesheet>
