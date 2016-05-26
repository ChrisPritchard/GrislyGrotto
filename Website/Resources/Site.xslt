<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="html" indent="yes"/>
    <xsl:template match="/">

      <html>
        <head>
          <title>Grisly Grotto - Deviant Minds Think Alike</title>
          <link rel="alternate" type="application/rss+xml" title="RSS 2.0" href="/Rss.aspx" />
          <link href="/Resources/Site.css" type="text/css" rel="Stylesheet" />
          <script src="/Resources/jquery-1.3.2.min.js" type="text/javascript" />
          <script src="/Resources/Site.js" type="text/javascript" />
          <script src="/Resources/ckeditor/ckeditor.js" type="text/javascript" />
          <script src="/Resources/jquery-ui-1.7.2.custom.min.js" type="text/javascript" />
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
            <a href="/?ShowUser=Christopher" class="userLink">Christopher</a>
            <span class="seperator">|</span>
            <a href="/?ShowUser=Peter" class="userLink">Peter</a>
            <span class="seperator">|</span>
            <a href="/?ShowUser=Ben" class="userLink">Ben</a>
            <span class="seperator">|</span>
            <a href="/?ShowUser=Kelly" class="userLink">Kelly</a>
          </div>
          
          <div class="loginBox">
            <xsl:choose>
              <xsl:when test="Page/@LoggedUser">
                <h3>
                  Welcome <xsl:value-of select="Page/@LoggedUser"/>
                </h3>
                <p>
                  <a href="#" onclick="newPost();return false;">Add new post</a>
                  <br/>
                  <a href="/?Action=Logout">Logout</a>
                </p>
                <xsl:if test="Page/Drafts/Post">
                <h4>Drafts</h4>
                  <ul>
                    <xsl:for-each select="Page/Drafts/Post">
                      <li>
                        <a href="?Post={@ID}">
                          <xsl:value-of select="@Title"/>
                        </a>
                      </li>
                    </xsl:for-each>
                  </ul>
                </xsl:if>
              </xsl:when>
              <xsl:otherwise>
                <form>
                  <xsl:if test="Page/@LastLoginNameTry">
                    <p class="error">Incorrect Username or Password. Please try again.</p>
                  </xsl:if>
                  <table border="0" align="right">
                    <tr>
                      <td>Username:</td>
                      <td>
                        <input type="text" name="LoginName" value="{Page/@LastLoginNameTry}" />
                      </td>
                    </tr>
                    <tr>
                      <td>Password:</td>
                      <td>
                        <input type="password" name="LoginPassword" value="" />
                      </td>
                    </tr>
                    <tr>
                      <td colspan="2" align="right">
                        <input type="submit" value="Login" />
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
                  <a href="?Post={@ID}&amp;ShowUser={@User}">
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
            <form>
              Search: <input type="text" name="SearchTerm" value="{Page/@SearchTerm}" /><input type="submit" value="Go" />
            </form>
          </div>

          <div class="postHistory">
            <span class="historyTitle">Post History</span>
            <ul>
              <xsl:for-each select="Page/PostHistory/MonthPostCount">
                <li>
                  <a href="?Year={@Year}&amp;Month={@Month}">
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
                    <a href="/?Post={@ID}" class="commentsLink">
                      Comments (<xsl:value-of select="@CommentCount"/>)
                    </a>
                  </xsl:if>
                  <xsl:if test="/Page/@LoggedUser = @User">
                    <a href="#" onclick="editPost({@ID});return false;" class="editLink">Edit</a>
                    <span class="seperator">|</span>
                    <a href="/?DeletePost={@ID}" onclick="return confirm('Are you sure you want to delete this post?')" class="deleteLink">Delete</a>
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
            <form>
              <input type="hidden" name="Post" value="{Page/Post/@ID}" />
              <h3>
                Author: <input type="text" name="Author" />
              </h3>
              <p>
                <textarea name="Content" rows="5" cols="40" />
              </p>
              <input type="submit" value="Add Comment" />
            </form>
          </xsl:if>

          <xsl:if test="Page/@LoggedUser">
            <div class="editor" style="display:none">
              <form action="default.aspx" method="POST">
                <input type="hidden" id="editorPost" name="Post" />
                <input type="hidden" id="loggedUser" name="LoggedUser" value="{Page/@LoggedUser}" />
                <h3>
                  Title: <input type="text" id="editorTitle" name="Title" />
                </h3>
                <p>
                  <textarea class="ckeditor"  id="editorContent" name="Content" rows="15" cols="80" />
                </p>
                <input type="submit" value="Submit" />
              </form>
              <p id="draftStatus">
                <a href="#" onclick="saveDraft(); return false;">Save draft</a>
              </p>
            </div>
          </xsl:if>
          
        </body>        
      </html>
      
    </xsl:template>
</xsl:stylesheet>
