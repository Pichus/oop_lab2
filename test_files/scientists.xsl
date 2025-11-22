<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" />

    <xsl:template match="/">
        <html>
            <head>
                <meta charset="UTF-8"/>
                <title>Кадри науковців</title>
                <style>
                    table {
                    border-collapse: collapse;
                    width: 100%;
                    }
                    th, td {
                    border: 1px solid #444;
                    padding: 6px;
                    text-align: left;
                    }
                    th {
                    background: #eee;
                    }
                </style>
            </head>

            <body>
                <h2>Кадри науковців (Звання)</h2>

                <table>
                    <tr>
                        <th>П.І.П.</th>
                        <th>Факультет</th>
                        <th>Кафедра</th>
                        <th>Науковий ступінь</th>
                        <th>Вчене звання</th>
                        <th>Дата присвоєння</th>
                    </tr>

                    <xsl:for-each select="Scientists/Scientist">
                        <tr>
                            <td><xsl:value-of select="FullName" /></td>
                            <td><xsl:value-of select="Faculty" /></td>
                            <td><xsl:value-of select="Department" /></td>
                            <td><xsl:value-of select="AcademicDegree" /></td>
                            <td><xsl:value-of select="AcademicRank" /></td>
                            <td><xsl:value-of select="RankDate" /></td>
                        </tr>
                    </xsl:for-each>

                </table>
            </body>
        </html>
    </xsl:template>

</xsl:stylesheet>
