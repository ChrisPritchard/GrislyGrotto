<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:output method="html" indent="yes"/>

	<xsl:include href="../Shared/Common.xslt"/>

	<xsl:template match="ViewData">

		<script type="text/javascript" src="/Scripts/jquery.form.js" />
		<script type="text/javascript" src="/Scripts/Blog/Editor.js" />

		<form id="CreateBlog" method="post" action="/Blog/CreateBlog">

			<xsl:if test="Blog/@BlogID">
				<xsl:attribute name="action">
					/Blog/EditBlog/<xsl:value-of select="Blog/@BlogID"/>
				</xsl:attribute>
			</xsl:if>

			<input type="hidden" name="AuthorID" value="{AuthorDetails//Author[@LoggedIn]/@AuthorID}" />

			<label for="Title">Title:&#160;</label>
			<input type="text" id="Title" name="Title" value="{Blog/@Title}" />
			<br/>
			<label for="Tags">Tags:&#160;</label>
			<input type="text" id="Tags" name="Tags" value="{Blog/@Tags}" />
			<br/>

			<textarea id="Content" name="Content" cols="80" rows="20" scrollbars="none">
				<xsl:value-of select="Blog/."/>
			</textarea>
			<br/>

			<input id="editorSubmit" type="submit" value="Submit" />

		</form>

		<form method="post" action="/Blog/UploadImage" enctype="multipart/form-data" id="ImageForm">
			<xsl:if test="Blog/@BlogID">
				<xsl:attribute name="action">
					/Blog/UploadImage/<xsl:value-of select="Blog/@BlogID"/>
				</xsl:attribute>
			</xsl:if>

			<a href="#" onclick="AllUserImages();" id="UserImagesLink">Show All User Image Paths</a>
			<div id="UserImagePaths"></div>
			<label for="ImagePath">Upload Image:&#160;</label>
			<input type="file" id="ImagePath" name="ImagePath" onchange=""/>
			<input type="submit" value="Upload" id="UploadButton" />

		</form>

	</xsl:template>

</xsl:stylesheet>
