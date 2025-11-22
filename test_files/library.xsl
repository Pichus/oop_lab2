<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8"/>

    <xsl:template match="/">
        <html>
            <head>
                <meta charset="utf-8"/>
                <title>Library - transformed</title>
                <style>
                    body { font-family: Arial, Helvetica, sans-serif; padding: 12px; }
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid #ccc; padding: 6px; text-align: left; vertical-align: top; }
                    th { background: #f3f3f3; }
                </style>
            </head>
            <body>
                <h1>Library</h1>
                <table>
                    <tr>
                        <th>ID</th>
                        <th>Type</th>
                        <th>Language</th>
                        <th>Title</th>
                        <th>Author</th>
                        <th>Faculty</th>
                        <th>Department</th>
                        <th>Annotation</th>
                        <th>Keywords</th>
                    </tr>

                    <xsl:for-each select="library/book">
                        <tr>
                            <td><xsl:value-of select="@id" /></td>
                            <td><xsl:value-of select="@type" /></td>
                            <td><xsl:value-of select="@language" /></td>

                            <td>
                                <xsl:value-of select="normalize-space(title)" />
                            </td>

                            <td>
                                <!-- якщо author/name відсутній, вивести весь author як текст -->
                                <xsl:choose>
                                    <xsl:when test="author/name">
                                        <xsl:value-of select="author/name" />
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <xsl:value-of select="normalize-space(author)" />
                                    </xsl:otherwise>
                                </xsl:choose>
                            </td>

                            <td><xsl:value-of select="author/faculty" /></td>
                            <td><xsl:value-of select="author/department" /></td>
                            <td><xsl:value-of select="normalize-space(annotation)" /></td>
                            <td>
                                <!-- розбити keywords через кому на окремі елементи, якщо потрібно -->
                                <xsl:value-of select="keywords" />
                            </td>
                        </tr>
                    </xsl:for-each>
                </table>
            </body>
        </html>
    </xsl:template>
</xsl:stylesheet>
