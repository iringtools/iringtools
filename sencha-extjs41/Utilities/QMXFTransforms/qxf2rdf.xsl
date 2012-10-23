<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet SYSTEM "entities.dtd">
<xsl:stylesheet version="1.0" exclude-result-prefixes="xsl"
		xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:rdf="&rdf;#"
		xmlns:rdfs="&rdfs;#"
		xmlns:owl="&owl;#"
		xmlns:xsd="&xsd;#"
		xmlns:dm="&dm;#"
		xmlns:rdl="&rdl;#"
		xmlns:tpl="&tpl;#"
		xmlns:oim="&oim;#"
		xmlns:qxl="&qxl;#"
		xmlns:qxf="&qxf;#" >
	<xsl:output method="xml"
		indent="yes"
		encoding="UTF-8"
		omit-xml-declaration="no"/>
	<xsl:param name="generate-template-id-prefix" select="''"/>
	<xsl:strip-space elements="*"/>
	<xsl:template name="extract-ns">
		<xsl:param name="uri"/>
		<xsl:value-of select="concat(substring-before($uri, '#'),
			'#')"/>
	</xsl:template>
	<xsl:template name="extract-qname">
		<xsl:param name="uri"/>
		<xsl:variable name="ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri" select="$uri"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="name" select="substring-after($uri, '#')"/>
		<xsl:choose>
			<!--
				@todo maybe load these mappings from
				an external source.
			-->
			<xsl:when test="$ns = '&xsd;#'">
				<xsl:value-of select="concat('xsd:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = '&rdf;#'">
				<xsl:value-of select="concat('rdf:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = '&rdfs;#'">
				<xsl:value-of select="concat('rdfs:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = '&owl;#'">
				<xsl:value-of select="concat('owl:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = '&dm;#'">
				<xsl:value-of select="concat('dm:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = '&rdl;#'">
				<xsl:value-of select="concat('rdl:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = '&tpl;#'">
				<xsl:value-of select="concat('tpl:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = '&oim;#'">
				<xsl:value-of select="concat('oim:', $name)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$name"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="qxl:item">
		<xsl:variable name="next"
			select="(following-sibling::*)[position() = 1]"/>
		<rdf:List>
			<rdf:first>
				<xsl:choose>
					<xsl:when test="count(@reference) > 0">
						<xsl:attribute namespace="&rdf;#" name="rdf:resource"><xsl:value-of select="@reference"/></xsl:attribute>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="string(.)"/>
					</xsl:otherwise>
				</xsl:choose>
			</rdf:first>
			<rdf:rest>
				<xsl:choose>
					<xsl:when test="$next">
						<xsl:apply-templates select="$next"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute namespace="&rdf;#" name="rdf:resource">&rdf;#nil</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
			</rdf:rest>
		</rdf:List>
	</xsl:template>
	<xsl:template match="qxl:list">
		<xsl:variable name="first"
			select="(qxl:list|qxl:item)[position() = 1]"/>
		<xsl:apply-templates select="$first"/>
	</xsl:template>
	<xsl:template match="qxf:property">
		<xsl:variable name="type-ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri" select="@instance-of"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="type-qname">
			<xsl:call-template name="extract-qname">
				<xsl:with-param name="uri" select="@instance-of"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:element namespace="{$type-ns}" name="{$type-qname}">
			<xsl:choose>
				<xsl:when test="@as = '&qxl;#literal'">
					<xsl:apply-templates
						select="(child::qxl:list|qxl:item)[position() = 1]"/>
				</xsl:when>
				<xsl:when test="@reference">
					<xsl:attribute name="rdf:resource">
						<xsl:value-of
							select="@reference"/>
					</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="@lang">
						<xsl:attribute
							namespace="&xsd;#"
							name="xml:lang">
							<xsl:value-of
								select="@lang"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@as">
						<xsl:attribute
							namespace="&rdf;#"
							name="rdf:datatype">
							<xsl:value-of
								select="@as"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:value-of select="string(.)"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>
	<!--
		Matches all non-specific relationships (for specific
		relationships, see below.
	-->
	<xsl:template match="qxf:relationship">
		<xsl:variable name="type-ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri" select="@instance-of"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="type-qname">
			<xsl:call-template name="extract-qname">
				<xsl:with-param name="uri" select="@instance-of"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="@instance-of = '&tpl;#R99514676016'">
				<xsl:variable name="property"
					select="qxf:property[@instance-of =
					'&tpl;#R25980496885']/@reference"/>
				<xsl:variable name="annotated"
					select="qxf:property[@instance-of =
					'&tpl;#R71961093547']/@reference"/>
				<xsl:variable name="value"
					select="string(qxf:property[@instance-of =
					'&tpl;#R49608530151'])"/>
				<xsl:variable name="lang"
					select="string(qxf:property[@instance-of =
					'&tpl;#R49608530151']/@xml:lang)"/>
				<xsl:variable name="type"
					select="string(qxf:property[@instance-of =
					'&tpl;#R49608530151']/@as)"/>
				<xsl:variable name="property-ns">
					<xsl:call-template name="extract-ns">
						<xsl:with-param name="uri" select="$property"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="property-qname">
					<xsl:call-template name="extract-qname">
						<xsl:with-param name="uri" select="$property"/>
					</xsl:call-template>
				</xsl:variable>
				<rdf:Description rdf:about="{$annotated}">
					<xsl:element namespace="{$property-ns}" name="{$property-qname}">
					    <xsl:if test="$type and not($lang)">
							<xsl:attribute namespace="&rdf;#" name="datatype"><xsl:value-of select="$type"/></xsl:attribute>
					    </xsl:if>
					    <xsl:if test="$lang">
							<xsl:attribute namespace="&xml;#" name="xml:lang"><xsl:value-of select="$lang"/></xsl:attribute>
					    </xsl:if>
					    <xsl:value-of select="$value"/>
					</xsl:element>
				</rdf:Description>
			</xsl:when>
			<xsl:otherwise>
				<xsl:element namespace="{$type-ns}" name="{$type-qname}">
					<xsl:variable name="id">
						<xsl:choose>
							<xsl:when test="@id"><xsl:value-of select="string(@id)"/></xsl:when>
							<xsl:when test="$generate-template-id-prefix != ''"><xsl:value-of select="concat($generate-template-id-prefix, generate-id(.))"/></xsl:when>
							<xsl:otherwise/>
						</xsl:choose>
					</xsl:variable>
					<xsl:choose>
						<xsl:when test="$id != ''">
							<xsl:attribute name="rdf:about">
								<xsl:value-of select="$id"/>
							</xsl:attribute>
						</xsl:when>
						<xsl:otherwise>
							<!-- do nothing -->
						</xsl:otherwise>
					</xsl:choose>
					<xsl:apply-templates/>
				</xsl:element>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!--
		Specifically matches relationships expressing RDF triples
	-->
	<xsl:template match="qxf:relationship[@instance-of='&rdf;#Statement']">
		<xsl:variable name="predicate-uri"
			select="qxf:property[@instance-of='&rdf;#predicate']/
				@reference"/>
		<xsl:variable name="predicate-ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri" select="$predicate-uri"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="predicate-qname">
			<xsl:call-template name="extract-qname">
				<xsl:with-param name="uri" select="$predicate-uri"/>
			</xsl:call-template>
		</xsl:variable>
		<rdf:Description rdf:about="{qxf:property[
				@instance-of='&rdf;#subject']/@reference}">
			<xsl:element namespace="{$predicate-ns}"
				name="{$predicate-qname}">
				<xsl:attribute namespace="&rdf;#"
					name="resource"><xsl:value-of
					select="qxf:property[
					@instance-of='&rdf;#object']/
					@reference"/></xsl:attribute>
			</xsl:element>
		</rdf:Description>
	</xsl:template>
	<!--
		Specifically matches rdf:type expressed as a relationship.
	-->
	<xsl:template match="qxf:relationship[@instance-of='&rdf;#Statement'
			and (string(qxf:property[@instance-of='&rdf;#predicate']/@reference) = '&rdf;#type' )]">
		<xsl:variable name="object-uri"
			select="qxf:property[@instance-of='&rdf;#object']/
				@reference"/>
		<xsl:if test="not($object-uri)">
		    <xsl:message terminate="yes">ERROR: no object qxf:property/@reference</xsl:message>
		</xsl:if>
		<xsl:if test="not(qxf:property[@instance-of='&rdf;#subject']/@reference)">
		    <xsl:message terminate="yes">ERROR: no subject qxf:property/@reference</xsl:message>
		</xsl:if>
		<xsl:variable name="object-ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri"
					select="$object-uri"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="object-qname">
			<xsl:call-template name="extract-qname">
				<xsl:with-param name="uri"
					select="$object-uri"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:element namespace="{$object-ns}"
			    name="{$object-qname}">
			<xsl:attribute namespace="&rdf;#"
				name="rdf:about"><xsl:value-of
				select="qxf:property[
				@instance-of='&rdf;#subject']/@reference
				"/></xsl:attribute>
		</xsl:element>
	</xsl:template>
	<xsl:template match="/qxf:qxf">
		<rdf:RDF>
			<xsl:if test="count(@xml:base) &gt; 0">
				<xsl:attribute namespace="&xml;#"
					name="xml:base"><xsl:value-of select="@xml:base"/></xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</rdf:RDF>
	</xsl:template>
	<xsl:template match="comment()">
		<xsl:copy/>
	</xsl:template>
	<xsl:template match="/">
		<xsl:apply-templates/>
	</xsl:template>
</xsl:stylesheet>
