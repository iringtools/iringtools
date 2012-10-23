<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.1"
                xmlns="http://ns.ids-adi.org/qxf/schema#"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:param name="dtoFilename"/>
  <!--<xsl:variable name="dtoFilename" select="'C:\ids-adi\camelot\Code\Adapter\AdapterService\XML\Line.xml'"/>-->
  <xsl:variable name="dtoList" select="document($dtoFilename)/*[1]"/>

  <xsl:template match="/Mapping">
    <xsl:variable name="qxf">
      <xsl:variable name="graphMaps" select="GraphMaps"/>
      <xsl:for-each select="$dtoList/*">
        <xsl:apply-templates select="$graphMaps/GraphMap">
          <xsl:with-param name="dtoIndex" select="position()"/>
        </xsl:apply-templates>
      </xsl:for-each>
    </xsl:variable>
    <xsl:element name="qxf">
      <xsl:for-each select="msxsl:node-set($qxf)/*">
        <xsl:if test="not(following-sibling::node()[.=string(current())])">
          <xsl:element name="relationship">
            <xsl:for-each select="*">
              <xsl:choose>
                <xsl:when test="name()='instance-of'">
                  <xsl:attribute name="instance-of">
                    <xsl:value-of select="."/>
                  </xsl:attribute>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:element name="property">
                    <xsl:for-each select="*">
                      <xsl:choose>
                        <xsl:when test="name()='value'">
                          <xsl:value-of select="."/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:attribute name="{name()}">
                            <xsl:value-of select="."/>
                          </xsl:attribute>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:for-each>
                  </xsl:element>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template match="GraphMaps/GraphMap">
    <xsl:param name="dtoIndex"/>    
    <xsl:if test="@name=name($dtoList/*[$dtoIndex])">
      <xsl:variable name="identifier" select="@identifier"/>
      <xsl:variable name="role" select="TemplateMaps/TemplateMap/RoleMaps/RoleMap[@propertyName=$identifier]"/>
      <xsl:variable name="roleName" select="$role/@name"/>
      <xsl:variable name="templateName" select="$role/../../@name"/>
      <xsl:variable name="dtoIdentifier" select="concat('http://www.example.com/data#', $dtoList/*[$dtoIndex]/*[name()=concat($templateName, '.', $roleName)])"/>      
      <xsl:call-template name="Classification">
        <xsl:with-param name="classId" select="@classId"/>
        <xsl:with-param name="identifier" select="$dtoIdentifier"/>
      </xsl:call-template>      
      <xsl:apply-templates select="TemplateMaps/TemplateMap">
        <xsl:with-param name="dtoIndex" select="$dtoIndex"/>
        <xsl:with-param name="classId" select="@classId"/>
        <xsl:with-param name="identifier" select="$dtoIdentifier"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <xsl:template match="TemplateMaps/TemplateMap">
    <xsl:param name="dtoIndex"/>
    <xsl:param name="classId"/>
    <xsl:param name="identifier"/>
    <xsl:choose>
      <xsl:when test="@type='Property'">
        <xsl:element name="relationship">
          <xsl:element name="instance-of">
            <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after(@templateId, 'tpl:'))"/>
          </xsl:element>
          <xsl:apply-templates select="RoleMaps/RoleMap">
            <xsl:with-param name="dtoIndex" select="$dtoIndex"/>
            <xsl:with-param name="identifier" select="$identifier"/>
            <xsl:with-param name="templateType" select="@type"/>
            <xsl:with-param name="templateId" select="@templateId"/>
            <xsl:with-param name="classRole" select="@classRole"/>
          </xsl:apply-templates>
          <xsl:call-template name="ReferenceProperty">
            <xsl:with-param name="role" select="@classRole"/>
            <xsl:with-param name="reference" select="$identifier"/>
          </xsl:call-template>
        </xsl:element>
      </xsl:when>
      <xsl:when test="@type='Relationship'">
        <xsl:apply-templates select="RoleMaps/RoleMap">
          <xsl:with-param name="dtoIndex" select="$dtoIndex"/>
          <xsl:with-param name="identifier" select="$identifier"/>
          <xsl:with-param name="templateType" select="@type"/>
          <xsl:with-param name="templateId" select="@templateId"/>
          <xsl:with-param name="classRole" select="@classRole"/>
        </xsl:apply-templates>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="RoleMaps/RoleMap">
    <xsl:param name="dtoIndex"/>
    <xsl:param name="identifier"/>
    <xsl:param name="templateType"/>
    <xsl:param name="templateId"/>
    <xsl:param name="classRole"/>
    <xsl:choose>
      <xsl:when test="$templateType='Property'">
        <xsl:variable name="dtoName" select="concat(../../@name, '.', @name)"/>
        <xsl:variable name="dtoValue" select="$dtoList/*[$dtoIndex]/*[name()=$dtoName]"/>
        <xsl:call-template name="ValueProperty">
          <xsl:with-param name="roleId" select="@roleId"/>
          <xsl:with-param name="dataType" select="@dataType"/>
          <xsl:with-param name="dtoValue" select="$dtoValue"/>
          <xsl:with-param name="valueList" select="@valueList"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$templateType='Relationship'">
        <xsl:apply-templates select="ClassMap">
          <xsl:with-param name="dtoIndex" select="$dtoIndex"/>
          <xsl:with-param name="identifier" select="$identifier"/>
          <xsl:with-param name="templateId" select="$templateId"/>
          <xsl:with-param name="roleMapRoleId" select="@roleId"/>
          <xsl:with-param name="classRole" select="$classRole"/>
        </xsl:apply-templates>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ClassMap">
    <xsl:param name="dtoIndex"/>
    <xsl:param name="identifier"/>
    <xsl:param name="templateId"/>
    <xsl:param name="roleMapRoleId"/>
    <xsl:param name="classRole"/>
    <!--
    <xsl:variable name="dtoIdentifierName">
      <xsl:call-template name="DtoIdentifierValues">
        <xsl:with-param name="dtoIndex" select="$dtoIndex"/>
        <xsl:with-param name="identifiers" select="@identifier"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="dtoIdentifierValue">
      <xsl:choose>
        <xsl:when test="$dtoIdentifierName=''">
          <xsl:value-of select="concat('http://rdl.rdswip.org/data#', substring-after(@classId, ':'))"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat('http://www.example.com/data#', $dtoIdentifierName)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    -->
    <xsl:variable name="dtoIdentifierValue" select="concat('http://rdl.rdswip.org/data#', substring-after(@classId, ':'))"/>
    <xsl:apply-templates select="TemplateMaps/TemplateMap">
      <xsl:with-param name="dtoIndex" select="$dtoIndex"/>
      <xsl:with-param name="identifier" select="$dtoIdentifierValue"/>
    </xsl:apply-templates>
    <xsl:element name="relationship">
      <xsl:element name="instance-of">
        <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after($templateId, 'tpl:'))"/>
      </xsl:element>
      <xsl:call-template name="ReferenceProperty">
        <xsl:with-param name="role" select="$roleMapRoleId"/>
        <xsl:with-param name="reference" select="$dtoIdentifierValue"/>
      </xsl:call-template>
      <xsl:call-template name="ReferenceProperty">
        <xsl:with-param name="role" select="$classRole"/>
        <xsl:with-param name="reference" select="$identifier"/>
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="Classification">
    <xsl:param name="classId"/>
    <xsl:param name="identifier"/>
    <xsl:element name="relationship">
      <xsl:element name="instance-of">
        <xsl:value-of select="'http://dm.rdswip.org/data#classification'"/>
      </xsl:element>
      <xsl:call-template name="ReferenceProperty">
        <xsl:with-param name="role" select="'dm:class'"/>
        <xsl:with-param name="reference" select="concat('http://rdl.rdswip.org/data#', substring-after($classId, 'rdl:'))"/>
      </xsl:call-template>
      <xsl:call-template name="ReferenceProperty">
        <xsl:with-param name="role" select="'dm:instance'"/>
        <xsl:with-param name="reference" select="$identifier"/>
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="DtoIdentifierValues">
    <xsl:param name="dtoIndex"/>
    <xsl:param name="identifiers"/>
    <xsl:choose>
      <xsl:when test="contains($identifiers, ',')">
        <xsl:variable name="identifier" select="substring-before($identifiers, ',')"/>
        <xsl:value-of select="$dtoList/*[$dtoIndex]/*[name()=$identifier]"/>
        <xsl:call-template name="DtoIdentifierValues">
          <xsl:with-param name="dtoIndex" select="$dtoIndex"/>
          <xsl:with-param name="identifiers" select="substring-after($identifiers, ',')"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$dtoList/*[$dtoIndex]/*[name()=$identifiers]"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="ReferenceProperty">
    <xsl:param name="role"/>
    <xsl:param name="reference"/>
    <xsl:element name="property">
      <xsl:element name="instance-of">
        <xsl:choose>
          <xsl:when test="contains($role,'dm:')">
            <xsl:value-of select="concat('http://dm.rdswip.org/data#', substring-after($role, 'dm:'))"/>
          </xsl:when>
          <xsl:when test="contains($role,'tpl:')">
            <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after($role, 'tpl:'))"/>
          </xsl:when>
        </xsl:choose>
      </xsl:element>
      <xsl:element name="reference">
        <xsl:value-of select="$reference"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="ValueProperty">
    <xsl:param name="roleId"/>
    <xsl:param name="dataType"/>
    <xsl:param name="dtoValue"/>
    <xsl:param name="valueList"/>
    <xsl:element name="property">
      <xsl:element name="instance-of">
        <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after($roleId, 'tpl:'))"/>
      </xsl:element>
      <xsl:choose>
        <xsl:when test="$valueList=''">
          <xsl:element name="as">
            <xsl:value-of select="concat('http://www.w3.org/2001/XMLSchema#', substring-after($dataType, 'xsd:'))"/>
          </xsl:element>
          <xsl:element name="value">
            <xsl:value-of select="$dtoValue"/>
          </xsl:element>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="valueMapList" select="/Mapping/ValueMaps/ValueMap[@valueList=$valueList]"/>
          <xsl:variable name="valueMap" select="$valueMapList[@internalValue=$dtoValue]"/>
          <xsl:variable name="modelURI" select="$valueMap/@modelURI"/>
          <xsl:element name="as">
            <xsl:value-of select="concat('http://rdl.rdswip.org/data#', substring-after($modelURI, 'rdl:'))"/>
          </xsl:element>
          <xsl:element name="value">
            <xsl:value-of select="$dtoValue"/>
          </xsl:element>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>

