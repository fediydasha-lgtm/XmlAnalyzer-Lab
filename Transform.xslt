<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" />
  
  <xsl:template match="/">
    <html>
      <head>
        <title>Дані про студентів</title>
        <style>
          body { font-family: Arial, sans-serif; padding: 20px; }
          table { border-collapse: collapse; width: 100%; margin-top: 20px; }
          th, td { border: 1px solid #dddddd; padding: 12px; text-align: left; }
          th { background-color: #f2f2f2; }
        </style>
      </head>
      <body>
        <h2>Таблиця студентів</h2>
        <table>
          <tr>
            <th>П.І.П.</th>
            <th>Факультет</th>
            <th>Кафедра</th>
            <th>Курс</th>
            <th>Дисципліни та оцінки</th>
          </tr>
          <!-- Перебираємо всіх студентів -->
          <xsl:for-each select="//Student">
            <tr>
              <td><xsl:value-of select="@FullName"/></td>
              <td><xsl:value-of select="@Faculty"/></td>
              <td><xsl:value-of select="@Department"/></td>
              <td><xsl:value-of select="@Course"/></td>
              <td>
                <!-- Перебираємо вкладені дисципліни -->
                <xsl:for-each select=".//Discipline">
                  <xsl:value-of select="@Name"/> - <xsl:value-of select="@Score"/> балів<br/>
                </xsl:for-each>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
