Ext.ns('AdapterManager');

AdapterManager.sppidConfigWizard = Ext.extend(Ext.Container, {
    scope: null,
    app: null,
    datalayer: null,
    iconCls: 'tabsSPPID',
    border: false,
    frame: false,


    constructor: function (config) {
        config = config || {};

        var wizard = this;
        var scopeName = config.scope;
        var appName = config.app;
        var datalayer = config.datalayer;
        var dbDict_SPPID;
        var dbPlantDict_SPPID;
        var SPPIDdbInfo;
        var dbTableNames_Stage;
        var userTableNames;
        var dataObjectsPane = new Ext.Panel({
            layout: 'border',
            id: scopeName + '.' + appName + '.dataObjectsPane',
            frame: false,
            border: false,
            items: [{
                xtype: 'panel',
                name: 'data-objects-pane',
                region: 'west',
                minWidth: 240,
                width: 300,
                split: true,
                layout: 'border',
                bodyStyle: 'background:#fff',
                items: [{
                    xtype: 'treepanel',
                    border: false,
                    autoScroll: true,
                    animate: true,
                    region: 'center',
                    lines: true,
                    frame: false,
                    enableDD: false,
                    containerScroll: true,
                    rootVisible: true,
                    root: {
                        text: 'Commodites',
                        nodeType: 'async',
                        iconCls: 'folder'
                    },
                    loader: new Ext.tree.TreeLoader(),
                    tbar: new Ext.Toolbar({
                        items: [{
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'button',
                            icon: 'Content/img/16x16/view-refresh.png',
                            text: 'Reload',
                            tooltip: 'Reload Data Objects',
                            handler: function () {
                                var editPane = dataObjectsPane.items.items[1];
                                var items = editPane.items.items;

                                for (var i = 0; i < items.length; i++) {
                                    items[i].destroy();
                                    i--;
                                }

                                Ext.Ajax.request({
                                    url: 'SPPID/DBDictionary',
                                    method: 'POST',
                                    params: {
                                        scope: scopeName,
                                        app: appName
                                    },
                                    success: function (response, request) {
                                        dbDict_SPPID = Ext.util.JSON.decode(response.responseText)[1].Data;
                                        dbPlantDict_SPPID = Ext.util.JSON.decode(response.responseText)[0].Data;

                                        if (dbDict_SPPID.ConnectionString)
                                            dbDict_SPPID.ConnectionString = Base64.decode(dbDict_SPPID.ConnectionString);

                                        var dbObjectsTree_SPPID = dataObjectsPane.items.items[0].items.items[0];

                                        if (dbDict_SPPID.dataObjects.length > 0) {
                                            // populate data source form
                                            SPPIDdbInfo = showTree_SPPID(dbObjectsTree_SPPID, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane);
                                        }
                                        else {
                                            dbObjectsTree_SPPID.disable();
                                            editPane = dataObjectsPane.items.items[1];
                                            if (!editPane) {
                                                var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                            }
                                            setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane, datalayer, dbPlantDict_SPPID);
                                        }
                                    },
                                    failure: function (response, request) {
                                        editPane = dataObjectsPane.items.items[1];
                                        if (!editPane) {
                                            var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                        }
                                        setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane, datalayer, dbPlantDict_SPPID);
                                        editPane.getLayout().setActiveItem(editPane.items.length - 1);
                                    }
                                });
                            }
                        }, {
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'button',
                            icon: 'Content/img/16x16/document-properties.png',
                            text: 'Edit Connection',
                            tooltip: 'Edit database connection',
                            handler: function (button) {
                                editPane = dataObjectsPane.items.items[1];
                                if (!editPane) {
                                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                }
                                Ext.Ajax.request({
                                    url: 'SPPID/GetConfiguration',
                                    timeout: 600000,
                                    method: 'POST',
                                    params: {
                                        scope: scopeName,
                                        app: appName
                                    },
                                    success: function (response, request) {
                                        // alert(response.responseText);
                                        dbPlantDict_SPPID = Ext.util.JSON.decode(response.responseText)
                                    },
                                    failure: function (response, request) {
                                        showDialog(660, 300, 'Saving Result', 'An error has occurred while saving the configuration.', Ext.Msg.OK, null);
                                    }
                                });
                                dbTableNames_Stage = setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane, datalayer, dbPlantDict_SPPID);
                            }
                        }, {
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'button',
                            icon: 'Content/img/16x16/document-save.png',
                            text: 'Save',
                            tooltip: 'Save the data objects tree to the back-end server',
                            formBind: true,
                            handler: function (button) {
                                editPane = dataObjectsPane.items.items[1];

                                if (!editPane) {
                                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                }

                                var dsConfigPane_SPPID = editPane.items.map[scopeName + '.' + appName + '.dsConfigPane_SPPID'];
                                var tablesSelectorPane = editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane'];
                                var dbObjectsTree_SPPID = dataObjectsPane.items.items[0].items.items[0];
                                var rootNode = dbObjectsTree_SPPID.getRootNode();
                                var treeProperty = getTreeJson_SPPID(dsConfigPane_SPPID, rootNode, SPPIDdbInfo, dbDict_SPPID, dataTypes_SPPID, tablesSelectorPane);


                                Ext.Ajax.request({
                                    url: 'SPPID/Trees',
                                    timeout: 600000,
                                    method: 'POST',
                                    params: {
                                        scope: scopeName,
                                        app: appName,
                                        tree: JSON.stringify(treeProperty)
                                    },
                                    success: function (response, request) {
                                        var rtext = response.responseText;
                                        var error = 'SUCCESS = FALSE';
                                        var index = rtext.toUpperCase().indexOf(error);
                                        if (index == -1) {
                                            showDialog(400, 100, 'Saving Result', 'Configuration has been saved successfully.', Ext.Msg.OK, null);
                                            var navpanel = Ext.getCmp('nav-panel');
                                            navpanel.onReload();
                                        }
                                        else {
                                            var msg = rtext.substring(index + error.length + 2, rtext.length - 1);
                                            showDialog(400, 100, 'Saving Result - Error', msg, Ext.Msg.OK, null);
                                        }
                                    },
                                    failure: function (response, request) {
                                        showDialog(660, 300, 'Saving Result', 'An error has occurred while saving the configuration.', Ext.Msg.OK, null);
                                    }
                                });
                            }
                        }]
                    }),
                    listeners: {
                        click: function (node, e) {
                            if (node.isRoot) {
                                editPane = dataObjectsPane.items.items[1];
                                if (!editPane) {
                                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                }

                                setTablesSelectorPane_SPPID(editPane, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane, dbTableNames_Stage);
                                return;
                            }
                            else if (!node)
                                return;

                            var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                            if (!editPane)
                                editPane = dataObjectsPane.items.items[1];

                            var nodeType = node.attributes.type;


                            if (!nodeType && node.attributes.attributes)
                                nodeType = node.attributes.attributes.type;


                            if (nodeType) {
                                editPane.show();
                                var editPaneLayout = editPane.getLayout();

                                switch (nodeType.toUpperCase()) {
                                    case 'DATAOBJECT':
                                        setDataObject_SPPID(editPane, node, dbDict_SPPID, dataObjectsPane, scopeName, appName);
                                        break;

                                    case 'KEYS':
                                        setKeysFolder(editPane, node, scopeName, appName);
                                        break;

                                    case 'KEYPROPERTY':
                                        setKeyProperty(editPane, node, scopeName, appName, dataTypes_SPPID);
                                        break;

                                    case 'PROPERTIES':
                                        setPropertiesFolder(editPane, node, scopeName, appName);
                                        break;

                                    case 'DATAPROPERTY':
                                        setDataProperty_SPPID(editPane, node, scopeName, appName, dataTypes_SPPID);
                                        break;

                                    case 'RELATIONSHIPS':
                                        setRelations(editPane, node, scopeName, appName);
                                        break;

                                    case 'RELATIONSHIP':
                                        setRelationFields(editPane, node, scopeName, appName);
                                        break;
                                }
                            }
                            else {
                                editPane.hide();
                            }
                        }
                    }
                }]
            }, {
                xtype: 'panel',
                name: 'editor-panel',
                border: 1,
                frame: false,
                id: scopeName + '.' + appName + '.editor-panel',
                region: 'center',
                layout: 'card'
            }]
        });


        Ext.apply(this, {
            id: scopeName + '.' + appName + '.-sppid-config',
            title: 'P & ID Configuration - ' + scopeName + '.' + appName,
            closable: true,
            border: false,
            frame: true,
            layout: 'fit',
            items: [dataObjectsPane]
        });

        Ext.Ajax.request({
            url: 'AdapterManager/DataType',
            timeout: 600000,
            method: 'GET',

            success: function (response, request) {
                var dataTypeName = Ext.util.JSON.decode(response.responseText);
                dataTypes_SPPID = new Array();
                var i = 0;
                while (!dataTypeName[i])
                    i++;
                while (dataTypeName[i]) {
                    dataTypes_SPPID.push([i, dataTypeName[i]]);
                    i++;
                }
            },
            failure: function (f, a) {
                if (a.response)
                    showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
            }
        });

        Ext.EventManager.onWindowResize(this.doLayout, this);

        Ext.Ajax.request({
            //url: 'SPPID/GetConfiguration',
            url: 'SPPID/DBDictionary',
            method: 'POST',
            params: {
                scope: scopeName,
                app: appName
            },
            success: function (response, request) {
                //dbDict_SPPID = Ext.util.JSON.decode(response.responseText);
                dbDict_SPPID = Ext.util.JSON.decode(response.responseText)[1].Data;
                dbPlantDict_SPPID = Ext.util.JSON.decode(response.responseText)[0].Data;

                if (dbDict_SPPID.ConnectionString)
                    dbDict_SPPID.ConnectionString = Base64.decode(dbDict_SPPID.ConnectionString);

                var tab = Ext.getCmp('content-panel');
                var rp = tab.items.map[scopeName + '.' + appName + '.-sppid-config'];
                var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                var dbObjectsTree_SPPID = dataObjectsPane.items.items[0].items.items[0];

                if (dbDict_SPPID.dataObjects.length > 0) {
                    //if (dbDict_SPPID.total > 0) {

                    // populate data source form
                    SPPIDdbInfo = showTree_SPPID(dbObjectsTree_SPPID, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane);
                    var abcdd = 5;
                }
                else {
                    dbObjectsTree_SPPID.disable();
                    editPane = dataObjectsPane.items.items[1];
                    if (!editPane) {
                        var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                    }
                    setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane, datalayer, dbPlantDict_SPPID);
                }
            },
            failure: function (response, request) {
                editPane = dataObjectsPane.items.items[1];
                if (!editPane) {
                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                }
                editPane.add(dsConfigPane_SPPID);
                editPane.getLayout().setActiveItem(editPane.items.length - 1);
            }
        });

        AdapterManager.sppidConfigWizard.superclass.constructor.apply(this, arguments);
    }
});


//*************************************
function setDsConfigFields_SPPID(dsConfigPane_SPPID, SPPIDdbInfo, dbDict_SPPID, dbPlantDict_SPPID) {
    var dsConfigForm_SPPId = dsConfigPane_SPPID.getForm();
    var Provider = null;

    if (dbDict_SPPID.Provider)
        Provider = dbDict_SPPID.Provider.toUpperCase();

    var dbName = dsConfigForm_SPPId.findField('dbstageName');
    var portNumber = dsConfigForm_SPPId.findField('stageportNumber');
    var dbServer = dsConfigForm_SPPId.findField('dbstageServer');
    var dbInstance = dsConfigForm_SPPId.findField('dbstageInstance');
    var serviceName = '';
    // var dbSchema = dsConfigForm_SPPId.findField('dbstageSchema');
    var userName = dsConfigForm_SPPId.findField('dbstageUserName');
    var password = dsConfigForm_SPPId.findField('dbstagePassword');
    var dbProvider = dsConfigForm_SPPId.findField('dbstageProvider');

    if (SPPIDdbInfo) {
        if (Provider) {
            if (Provider.indexOf('MSSQL') > -1) {
                portNumber.hide();
                //host.hide();
                // serviceName.hide();

                dbServer.setValue(SPPIDdbInfo.dbServer);
                dbServer.show();
                dbInstance.setValue(SPPIDdbInfo.dbInstance);
                dbInstance.show();
                dbName.setValue(SPPIDdbInfo.dbName);
                dbName.show();
                dbProvider.setValue(dbDict_SPPID.Provider);
                //host.setValue(SPPIDdbInfo.dbServer);
                portNumber.setValue(SPPIDdbInfo.portNumber);
                userName.setValue(SPPIDdbInfo.dbUserName);
                password.setValue(SPPIDdbInfo.dbPassword);
                // dbSchema.setValue(dbDict_SPPID.SchemaName);
            }
        }
    }
    else {
        dbServer.setValue('localhost');
        dbServer.show();
        dbInstance.setValue('default');
        dbInstance.show();
        //dbSchema.setValue('dbo');
        portNumber.setValue('1433');
        portNumber.hide();

        dbName.setValue('');
        dbName.clearInvalid();
        dbName.show();
        userName.setValue('');
        password.setValue('');
        dbProvider.setValue('MsSql2008');

        //serviceName.hide();

        userName.clearInvalid();
        password.clearInvalid();
    }

    var dbName = dsConfigForm_SPPId.findField('dbName');
    var portNumber = dsConfigForm_SPPId.findField('portNumber');
    var host = dsConfigForm_SPPId.findField('dbhost');
    var dbServer = dsConfigForm_SPPId.findField('dbServer');
    var dbInstance = dsConfigForm_SPPId.findField('dbInstance');
    var serviceName = dsConfigPane_SPPID.items.items[0].items.items[9];
    var dbSchema = dsConfigForm_SPPId.findField('dbSchema');
    var userName = dsConfigForm_SPPId.findField('dbUserName');
    var password = dsConfigForm_SPPId.findField('dbPassword');
    var dbProvider = dsConfigForm_SPPId.findField('dbProvider');

    var dbPIDUserName = dsConfigForm_SPPId.findField('dbPIDUserName');
    var dbPIDPassword = dsConfigForm_SPPId.findField('dbPIDPassword');

    var dbPIDDataDicUserName = dsConfigForm_SPPId.findField('dbPIDDataDicUserName');
    var dbPIDDataDicPassword = dsConfigForm_SPPId.findField('dbPIDDataDicPassword');

    var dbOraPlantUserName = dsConfigForm_SPPId.findField('dbOraPlantUserName');
    var dbOraPlantPassword = dsConfigForm_SPPId.findField('dbOraPlantPassword');

    var dbPlantDataDicUserName = dsConfigForm_SPPId.findField('dbPlantDataDicUserName');
    var dbPlantDataDicPassword = dsConfigForm_SPPId.findField('dbPlantDataDicPassword');

    var dbplantName = dsConfigForm_SPPId.findField('dbplantName');
    var dbplantServer = dsConfigForm_SPPId.findField('dbplantServer');
    var plantuserName = dsConfigForm_SPPId.findField('dbplantUserName');
    var plantpassword = dsConfigForm_SPPId.findField('dbplantPassword');
    var dbplantProvider = dsConfigForm_SPPId.findField('dbplantProvider');


    var OraclePane = dsConfigPane_SPPID.items.items[2];
    var plantDatabase = dsConfigPane_SPPID.items.items[1];

    var SPPIDPlantdbInfo;

    if (dbPlantDict_SPPID.Provider.indexOf('ORACLE') > -1) {

        SPPIDPlantdbInfo = getdbPlantObjects(dbPlantDict_SPPID.SiteConnectionString, SPPIDPlantdbInfo, dbPlantDict_SPPID.Provider);

        dbName.hide();
        dbServer.hide();
        dbInstance.hide();
        dbServer.setValue(SPPIDPlantdbInfo.dbServer);
        dbInstance.setValue(SPPIDPlantdbInfo.dbInstance);
        dbName.setValue(SPPIDPlantdbInfo.dbName);
        userName.setValue(SPPIDPlantdbInfo.dbUserName);
        password.setValue(SPPIDPlantdbInfo.dbPassword);
        dbProvider.setValue(dbPlantDict_SPPID.Provider);
        dbSchema.setValue(SPPIDPlantdbInfo.SchemaName);
        host.setValue(SPPIDPlantdbInfo.dbServer);
        host.show();

        serviceName.show();
        creatRadioField(serviceName, serviceName.id, SPPIDPlantdbInfo.dbInstance, SPPIDPlantdbInfo.serName);

        setOraclePanevalue(dbPIDUserName, dbPIDPassword, dbPIDDataDicUserName, dbPIDDataDicPassword, dbOraPlantUserName, dbOraPlantPassword, dbPlantDataDicUserName, dbPlantDataDicPassword, dbPlantDict_SPPID);

        OraclePane.show();

        plantpassword.setValue('');
        plantpassword.allowBlank = true;

        plantuserName.setValue('');
        plantuserName.allowBlank = true;

        dbplantName.setValue('');
        dbplantName.allowBlank = true;
        
        dbplantServer.setValue('');
        dbplantServer.allowBlank = true;

        plantDatabase.hide();

        portNumber.setValue(SPPIDPlantdbInfo.portNumber);
        portNumber.show();
    }
    else {

        OraclePane.hide();

        SPPIDPlantdbInfo = getdbPlantObjects(dbPlantDict_SPPID.SiteConnectionString, SPPIDPlantdbInfo, dbPlantDict_SPPID.Provider);

        portNumber.hide();
        host.hide();
        serviceName.hide();

        dbServer.setValue(SPPIDPlantdbInfo.dbServer);
        dbServer.show();
        dbInstance.setValue(SPPIDPlantdbInfo.dbInstance);
        dbInstance.show();
        dbName.setValue(SPPIDPlantdbInfo.dbName);
        dbName.show();
        dbProvider.setValue(dbPlantDict_SPPID.Provider);
        host.setValue(SPPIDPlantdbInfo.dbServer);
        portNumber.setValue(SPPIDPlantdbInfo.portNumber);
        userName.setValue(SPPIDPlantdbInfo.dbUserName);
        password.setValue(SPPIDPlantdbInfo.dbPassword);
        dbSchema.hide();
        // dbSchema.setValue(dbDict.SchemaName);

        SPPIDPlantdbInfo = {}
        SPPIDPlantdbInfo = getdbPlantObjects(dbPlantDict_SPPID.PlantConnectionString, SPPIDPlantdbInfo, dbPlantDict_SPPID.Provider);

        dbplantServer.setValue(SPPIDPlantdbInfo.dbServer);
        dbplantServer.show();

        dbplantName.setValue(SPPIDPlantdbInfo.dbName);
        dbplantName.show();
        dbplantProvider.setValue(dbPlantDict_SPPID.Provider);
        //host.setValue(SPPIDdbInfo.dbServer);
        plantuserName.setValue(SPPIDPlantdbInfo.dbUserName);
        plantpassword.setValue(SPPIDPlantdbInfo.dbPassword);
        // dbSchema.setValue(dbDict_SPPID.SchemaName);

        plantDatabase.show();

    }
};


function setOraclePanevalue(dbPIDUserName, dbPIDPassword, dbPIDDataDicUserName, dbPIDDataDicPassword, dbOraPlantUserName, dbOraPlantPassword, dbPlantDataDicUserName, dbPlantDataDicPassword, connectionStrings) {

    var PIDConnectionString = connectionStrings.PIDConnectionString;
    var connStrParts = PIDConnectionString.split(';');

    for (var i = 0; i < connStrParts.length; i++) {
        var pair = connStrParts[i].split('=');
        switch (pair[0].toUpperCase()) {
            case 'USER ID':
                dbPIDUserName.setValue(pair[1]);
                break;
            case 'PASSWORD':
                dbPIDPassword.setValue(pair[1]);
                break;
        }
    }
    var PIDDataDicConnectionString = connectionStrings.PIDDataDicConnectionString;
    connStrParts = PIDDataDicConnectionString.split(';');
    for (var i = 0; i < connStrParts.length; i++) {
        var pair = connStrParts[i].split('=');
        switch (pair[0].toUpperCase()) {
            case 'USER ID':
                dbPIDDataDicUserName.setValue(pair[1]);
                break;
            case 'PASSWORD':
                dbPIDDataDicPassword.setValue(pair[1]);
                break;
        }
    }

    var PlantConnectionString = connectionStrings.PlantConnectionString;
    connStrParts = PlantConnectionString.split(';');
    for (var i = 0; i < connStrParts.length; i++) {
        var pair = connStrParts[i].split('=');
        switch (pair[0].toUpperCase()) {
            case 'USER ID':
                dbOraPlantUserName.setValue(pair[1]);
                break;
            case 'PASSWORD':
                dbOraPlantPassword.setValue(pair[1]);
                break;
        }
    }

    var PlantDataDicConnectionString = connectionStrings.PlantDataDicConnectionString;
    connStrParts = PlantDataDicConnectionString.split(';');
    for (var i = 0; i < connStrParts.length; i++) {
        var pair = connStrParts[i].split('=');
        switch (pair[0].toUpperCase()) {
            case 'USER ID':
                dbPlantDataDicUserName.setValue(pair[1]);
                break;
            case 'PASSWORD':
                dbPlantDataDicPassword.setValue(pair[1]);
                break;
        }
    }
};

function getdbPlantObjects(getdbPlantObjects, SPPIDPlantdbInfo, Provider) {
    SPPIDPlantdbInfo = {};
    var connStrParts = getdbPlantObjects.split(';');
    var provider = Provider.toUpperCase();
    for (var i = 0; i < connStrParts.length; i++) {
        var pair = connStrParts[i].split('=');
        switch (pair[0].toUpperCase()) {
            case 'DATA SOURCE':
                if (provider.indexOf('MSSQL') > -1) {
                    var dsValue = pair[1].split('\\');
                    SPPIDPlantdbInfo.dbServer = (dsValue[0].toLowerCase() == '.' ? 'localhost' : dsValue[0]);
                    SPPIDPlantdbInfo.dbInstance = dsValue[1];
                    SPPIDPlantdbInfo.portNumber = 1433;
                    SPPIDPlantdbInfo.serName = '';
                }
                else if (provider.indexOf('MYSQL') > -1) {
                    SPPIDPlantdbInfo.dbServer = (pair[1].toLowerCase() == '.' ? 'localhost' : pair[1]);
                    SPPIDPlantdbInfo.portNumber = 3306;
                }
                else if (provider.indexOf('ORACLE') > -1) {
                    var dsStr = connStrParts[i].substring(12, connStrParts[i].length);
                    var dsValue = dsStr.split('=');
                    for (var j = 0; j < dsValue.length; j++) {
                        dsValue[j] = dsValue[j].substring(dsValue[j].indexOf('(') + 1, dsValue[j].length);
                        switch (dsValue[j].toUpperCase()) {
                            case 'HOST':
                                var server = dsValue[j + 1];
                                var port = dsValue[j + 2];
                                var index = server.indexOf(')');
                                server = server.substring(0, index);
                                SPPIDPlantdbInfo.portNumber = port.substring(0, 4);
                                SPPIDPlantdbInfo.dbServer = (server.toLowerCase() == '.' ? 'localhost' : server);
                                break;
                            case 'SERVICE_NAME':
                                var sername = dsValue[j + 1];
                                index = sername.indexOf(')');
                                SPPIDPlantdbInfo.dbInstance = sername.substring(0, index);
                                SPPIDPlantdbInfo.serName = 'SERVICE_NAME';
                                break;
                            case 'SID':
                                var sername = dsValue[j + 1];
                                index = sername.indexOf(')');
                                SPPIDPlantdbInfo.dbInstance = sername.substring(0, index);
                                SPPIDPlantdbInfo.serName = 'SID';
                                break;
                        }
                    }
                }
                break;
            case 'INITIAL CATALOG':
                SPPIDPlantdbInfo.dbName = pair[1];
                break;
            case 'USER ID':
                SPPIDPlantdbInfo.dbUserName = pair[1];
                SPPIDPlantdbInfo.SchemaName = pair[1];
                break;
            case 'PASSWORD':
                SPPIDPlantdbInfo.dbPassword = pair[1];
                break;
        }
    }
    return SPPIDPlantdbInfo;
};
function changeConfigOracle(host, dbSchema, userName, password, serviceName, OraclePane, plantDatabase) {
    host.setValue('');
    host.clearInvalid();

    host.show();

    dbSchema.setValue('');
    dbSchema.clearInvalid();

    userName.setValue('');
    userName.clearInvalid();

    password.setValue('');
    password.clearInvalid();
    serviceName.show();
    creatRadioField(serviceName, serviceName.id, '', '', 1);
    for (var i = 0; i < OraclePane.items.length; i++) {
        for (var j = 0; j < OraclePane.items.items[0].items.length; j++) {
            Ext.getCmp(OraclePane.items.items[i].items.items[j].id).show();
        }
    }
    OraclePane.show();
    for (var i = 0; i < plantDatabase.items.length; i++) {
        if ((Ext.getCmp(plantDatabase.items.items[i].id).getValue() == '') && (plantDatabase.items.items[i].inputType != 'password')) {
            Ext.getCmp(plantDatabase.items.items[i].id).setValue(' ');
        }
        if (plantDatabase.items.items[i].inputType == 'password') {
            Ext.getCmp(plantDatabase.items.items[i].id).allowBlank = true
        }
        Ext.getCmp(plantDatabase.items.items[i].id).hide();
    }

    plantDatabase.hide();

}

function changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password, plantDatabase, OraclePane) {
    dbName.setValue('');
    dbName.clearInvalid();
    dbName.show();

    dbServer.setValue('localhost');
    dbServer.show();

    dbInstance.setValue('default');
    dbInstance.show();

    dbSchema.setValue('dbo');

    userName.setValue('');
    userName.clearInvalid();

    password.setValue('');
    password.clearInvalid();

    for (var i = 0; i < plantDatabase.items.length; i++) {
        Ext.getCmp(plantDatabase.items.items[i].id).show();
    }
    plantDatabase.show();

    for (var i = 0; i < OraclePane.items.length; i++) {
        //OraclePane.items.items[0].items.items[0]
        for (var j = 0; j < OraclePane.items.items[0].items.length; j++) {
            if (OraclePane.items.items[i].items.items[j].inputType != 'password') {
                Ext.getCmp(OraclePane.items.items[i].items.items[j].id).setValue(' ');
                Ext.getCmp(OraclePane.items.items[i].items.items[j].id).hide();
            }
        }
    }
    OraclePane.hide();
}

function setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane, datalayer, dbPlantDict_SPPID) {
    if (editPane) {
        if (editPane.items.map[scopeName + '.' + appName + '.dsConfigPane_SPPID']) {
            var dsConfigPanel = editPane.items.map[scopeName + '.' + appName + '.dsConfigPane_SPPID'];

            if (dsConfigPanel) {
                var panelIndex = editPane.items.indexOf(dsConfigPanel);
                editPane.getLayout().setActiveItem(panelIndex);
                return;
            }
        }



        var dsConfigPane_SPPID = new Ext.FormPanel({
            labelWidth: 150,
            id: scopeName + '.' + appName + '.dsConfigPane_SPPID',
            frame: false,
            border: false,
            autoScroll: true,
            title: "Configure SP & ID Data Source",
            bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
            monitorValid: true,
            defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
            items: [{
                xtype: 'fieldset',
                id: 'siteDatabase',
                title: "SP & ID Site Database Details",
                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
                items: [{
                    xtype: 'combo',
                    fieldLabel: 'Database Provider',
                    hiddenName: 'dbProvider',
                    allowBlank: false,
                    store: new Ext.data.SimpleStore({
                        fields: ['Provider'],
                        data: [
                      ["MSSQL2000", "MSSQL2000"], ["MSSQL2005", "MSSQL2005"], ["MSSQL2008", "MSSQL2008"], ["ORACLE8i", "ORACLE8i"], ["ORACLE9i", "ORACLE9i"], ["ORACLE10g", "ORACLE10g"]
                          ]
                    }),
                    selectOnFocus: true,
                    mode: 'local',
                    editable: false,
                    value: 'MsSql2008',
                    triggerAction: 'all',
                    displayField: 'Provider',
                    valueField: 'Provider',
                    listeners: { 'select': function (combo, record, index) {
                        var dbProvider = record.data.Provider.toUpperCase();
                        var dbName = dsConfigPane_SPPID.getForm().findField('dbName');
                        var portNumber = dsConfigPane_SPPID.getForm().findField('portNumber');
                        var host = dsConfigPane_SPPID.getForm().findField('dbhost');
                        var dbServer = dsConfigPane_SPPID.getForm().findField('dbServer');
                        var dbInstance = dsConfigPane_SPPID.getForm().findField('dbInstance');
                        var serviceName = dsConfigPane_SPPID.items.items[0].items.items[9];
                        var dbSchema = dsConfigPane_SPPID.getForm().findField('dbSchema');
                        var userName = dsConfigPane_SPPID.getForm().findField('dbUserName');
                        var password = dsConfigPane_SPPID.getForm().findField('dbPassword');
                        var OraclePane = dsConfigPane_SPPID.items.items[2];
                        var plantDatabase = dsConfigPane_SPPID.items.items[1];


                        if (dbProvider.indexOf('ORACLE') > -1) {
                            if (dbName.hidden == false) {
                                dbName.hide();
                                dbServer.hide();
                                dbInstance.hide();
                            }

                            if (host.hidden == true) {
                                if (dbDict_SPPID.Provider) {
                                    if (dbDict_SPPID.Provider.toUpperCase().indexOf('ORACLE') > -1) {
                                        host.setValue(SPPIDdbInfo.dbServer);
                                        serviceName.show();
                                        creatRadioField(serviceName, serviceName.id, SPPIDdbInfo.dbInstance, SPPIDdbInfo.serName);
                                        host.show();
                                        userName.setValue(SPPIDdbInfo.dbUserName);
                                        password.setValue(SPPIDdbInfo.dbPassword);
                                        dbSchema.setValue(dbDict_SPPID.SchemaName);

                                    }
                                    else
                                        changeConfigOracle(host, dbSchema, userName, password, serviceName, OraclePane, plantDatabase);


                                }
                                else {
                                    changeConfigOracle(host, dbSchema, userName, password, serviceName, OraclePane, plantDatabase);
                                }

                                portNumber.setValue('1521');
                                portNumber.show();
                            }
                        }
                        else if (dbProvider.indexOf('MSSQL') > -1) {
                            if (host.hidden == false) {
                                portNumber.hide();
                                host.hide();
                                serviceName.hide();
                            }

                            if (dbName.hidden == true) {
                                if (dbDict_SPPID.Provider) {
                                    if (dbDict_SPPID.Provider.toUpperCase().indexOf('MSSQL') > -1) {
                                        dbName.setValue(SPPIDdbInfo.dbName);
                                        dbServer.setValue(SPPIDdbInfo.dbServer);
                                        dbInstance.setValue(SPPIDdbInfo.dbInstance);
                                        dbName.show();
                                        dbServer.show();
                                        dbInstance.show();
                                        dbSchema.setValue(dbDict_SPPID.SchemaName);
                                        userName.setValue(SPPIDdbInfo.dbUserName);
                                        password.setValue(SPPIDdbInfo.dbPassword);
                                    }
                                    else
                                        changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password, plantDatabase, OraclePane);
                                }
                                else
                                    changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password, plantDatabase, OraclePane);
                            }

                            portNumber.setValue('1433');

                            changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password, plantDatabase, OraclePane);
                        }
                    }
                    }
                }, {
                    xtype: 'textfield',
                    name: 'dbServer',
                    fieldLabel: 'Database Server',
                    value: 'localhost',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbhost',
                    fieldLabel: 'Host Name',
                    hidden: true,
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'portNumber',
                    fieldLabel: 'Port Number',
                    hidden: true,
                    value: '1521',
                    allowBlank: false
                }, {
                    name: 'dbInstance',
                    xtype: 'textfield',
                    fieldLabel: 'Database Instance',
                    value: 'default',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbName',
                    fieldLabel: 'Database Name',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbUserName',
                    fieldLabel: 'User Name',
                    allowBlank: false,
                    listeners: { 'change': function (field, newValue, oldValue) {
                        var dbProvider = dsConfigPane_SPPID.getForm().findField('dbProvider').getValue().toUpperCase();
                        if (dbProvider.indexOf('ORACLE') > -1) {
                            var dbSchema = dsConfigPane_SPPID.getForm().findField('dbSchema');
                            dbSchema.setValue(newValue);
                            dbSchema.show();
                        }
                    }
                    }
                }, {
                    xtype: 'textfield',
                    inputType: 'password',
                    name: 'dbPassword',
                    fieldLabel: 'Password',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbSchema',
                    fieldLabel: 'Schema Name',
                    value: 'dbo',
                    allowBlank: false
                }, {
                    xtype: 'panel',
                    id: scopeName + '.' + appName + '.servicename',
                    name: 'serviceName',
                    layout: 'fit',
                    anchor: '100% - 1',
                    border: false,
                    frame: false
                },
           {
               xtype: 'checkbox',
               name: 'isPlantSchemaSame',
               fieldLabel: 'Site and Plant Databases are same',
               listeners: {
                   check: function (checkbox, checked) {
                       var siteDatabase = Ext.getCmp('siteDatabase').items
                       var plantDatabase = Ext.getCmp('plantDatabase').items
                       if (checked == true) {
                           for (var i = 0; i < siteDatabase.length - 1; i++) {
                               Ext.getCmp(plantDatabase.items[i].id).setValue(Ext.getCmp(siteDatabase.items[i].id).getValue());
                               Ext.getCmp(plantDatabase.items[i].id).disable(true);
                               //Ext.getCmp('dfd').items.items[i]Ext.getCmp(siteDatabase.items[5].id).getValue()
                           }
                       }
                       else {
                           for (var i = 0; i < siteDatabase.length - 1; i++) {
                               Ext.getCmp(plantDatabase.items[i].id).enable(true);
                               // Ext.getCmp(plantDatabase.items[i].id).setValue(Ext.getCmp(siteDatabase.items[i].id).originalValue);
                           }
                       }
                   }
               }


           }]
            },

            {
                xtype: 'fieldset',
                id: 'plantDatabase',
                title: " SP & ID Plant Database Details",
                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
                items: [{
                    xtype: 'textfield',
                    fieldLabel: 'Database Provider',
                    name: 'dbplantProvider',
                    allowBlank: false,
                    readOnly: true,
                    value: 'MsSql2008'


                }, {
                    xtype: 'textfield',
                    name: 'dbplantServer',
                    fieldLabel: 'Database Server',
                    value: 'localhost',
                    allowBlank: false
                }, {
                    name: 'dbplantInstance',
                    xtype: 'textfield',
                    fieldLabel: 'Database Instance',
                    value: 'default',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbplantName',
                    fieldLabel: 'Database Name',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbplantUserName',
                    fieldLabel: 'User Name',
                    allowBlank: false
                },
                {
                    xtype: 'textfield',
                    inputType: 'password',
                    name: 'dbplantPassword',
                    fieldLabel: 'Password',
                    allowBlank: false
                }]
            },

            { xtype: 'fieldset',
                id: 'OracleDatabase',
                title: "Oracle Schema Details",
                layout: "column",
                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 250, style: 'margin-left: 5px; margin-bottom: 5px;' },
                hidden: true,
                items: [{
                    xtype: 'fieldset',
                    columnWidth: .5,
                    title: "PID Schema",
                    padding: 3,
                    items: [{
                        xtype: 'textfield',
                        fieldLabel: 'User Name',
                        name: 'dbPIDUserName',
                        //  value: 'RUSSELCITY_PILOTPID'
                        value: 'dfgdfh'
                    },
                   {
                       xtype: 'textfield',
                       inputType: 'password',
                       fieldLabel: 'Password',
                       name: 'dbPIDPassword'
                       // value: 'RUSSELCITY_PILOTPID'

                   }]
                },
                 {
                     xtype: 'fieldset',
                     columnWidth: .5,
                     padding: 3,
                     title: "PID Datadictionary",

                     items: [{
                         xtype: 'textfield',
                         fieldLabel: 'User Name',
                         name: 'dbPIDDataDicUserName'
                         //value: 'RUSSELCITY_PILOTPIDD'
                     },
                        {
                            xtype: 'textfield',
                            inputType: 'password',
                            fieldLabel: 'Password',
                            name: 'dbPIDDataDicPassword'
                            //value: 'RUSSELCITY_PILOTPIDD'
                        }]
                 },
                    {
                        xtype: 'fieldset',
                        columnWidth: .5,
                        padding: 5,
                        title: "Plant Schema",
                        items: [{
                            xtype: 'textfield',
                            fieldLabel: 'User Name',
                            name: 'dbOraPlantUserName',
                           // value: 'RUSSELCITY_PILOT'
                        },
                        {
                            xtype: 'textfield',
                            inputType: 'password',
                            fieldLabel: 'Password',
                            name: 'dbOraPlantPassword',
                           // value: 'RUSSELCITY_PILOT'
                        }]
                    },
                    {
                        xtype: 'fieldset',
                        columnWidth: .5,
                        padding: 5,
                        title: "Plant Datadictionary",
                        items: [{
                            xtype: 'textfield',
                            fieldLabel: 'User Name',
                            name: 'dbPlantDataDicUserName',
                          //  value: 'RUSSELCITY_PILOTD'
                        },
                        {
                            xtype: 'textfield',
                            inputType: 'password',
                            fieldLabel: 'Password',
                            name: 'dbPlantDataDicPassword',
                          //  value: 'RUSSELCITY_PILOTD'
                        }]
                    }]
            },
                {
                    xtype: 'fieldset',
                    id: 'staggingDatabase',
                    title: " Stagging Database Details",
                    defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
                    items: [{
                        xtype: 'textfield',
                        fieldLabel: 'Database Provider',
                        name: 'dbstageProvider',
                        allowBlank: false,
                        readOnly: true,
                        value: 'MsSql2008'


                    }, {
                        xtype: 'textfield',
                        name: 'dbstageServer',
                        fieldLabel: 'Database Server',
                        value: 'localhost',
                        allowBlank: false
                    }, {
                        xtype: 'textfield',
                        name: 'stageportNumber',
                        fieldLabel: 'Port Number',
                        hidden: true,
                        value: '1521',
                        allowBlank: false
                    }, {
                        name: 'dbstageInstance',
                        xtype: 'textfield',
                        fieldLabel: 'Database Instance',
                        value: 'default',
                        allowBlank: false
                    }, {
                        xtype: 'textfield',
                        name: 'dbstageName',
                        fieldLabel: 'Database Name',
                        allowBlank: false
                    }, {
                        xtype: 'textfield',
                        name: 'dbstageUserName',
                        fieldLabel: 'User Name',
                        allowBlank: false

                    }, {
                        xtype: 'textfield',
                        inputType: 'password',
                        name: 'dbstagePassword',
                        fieldLabel: 'Password',
                        allowBlank: false
                    }]
                }],
            tbar: new Ext.Toolbar({
                items: [{
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/document-properties.png',
                    text: 'Connect',
                    tooltip: 'Connect',
                    handler: function (f) {
                        var dbProvider = dsConfigPane_SPPID.getForm().findField('dbProvider').getValue().toUpperCase();
                        var dbName = dsConfigPane_SPPID.getForm().findField('dbName');
                        var host = dsConfigPane_SPPID.getForm().findField('dbhost');
                        var portNumber = dsConfigPane_SPPID.getForm().findField('dbportNumber');
                        var dbServer = dsConfigPane_SPPID.getForm().findField('dbServer');
                        var dbInstance = dsConfigPane_SPPID.getForm().findField('dbInstance');
                        var serviceNamePane = dsConfigPane_SPPID.items.items[0].items.items[9];
                        var dbSchema = dsConfigPane_SPPID.getForm().findField('dbSchema');

                        var servieName = '';
                        var serName = '';
                        var _datalayer = '';

                        if (dbProvider.indexOf('ORACLE') > -1) {
                            dbServer.setValue(host.getValue());
                            dbName.setValue(dbSchema.getValue());
                            servieName = serviceNamePane.items.items[0].value;
                            serName = serviceNamePane.items.items[0].serName;
                            dbInstance.setValue(servieName);
                        }
                        else if (dbProvider.indexOf('MSSQL') > -1) {
                            host.setValue(dbServer.getValue());
                            serviceName = dbInstance.getValue();
                        }

                        dsConfigPane_SPPID.getForm().submit({
                            url: 'SPPID/UpdateConfig',
                            timeout: 600000,
                            params: {
                                scope: scopeName,
                                app: appName,
                                serName: serName,
                                //                                dbplantUserName: dbplantUsername,
                                //                               dbplantPassword: dbplantPassword,
                                //                               dbplantServer: dbplantServer,
                                //                               dbplantInstance: dbplantInstance,
                                //                               dbplantName: dbplantName,
                                _datalayer: datalayer
                            },
                            success: function (f, a) {
                                dbTableNames_Stage = Ext.util.JSON.decode(a.response.responseText);
                                //var tab = Ext.getCmp('content-panel');
                                //var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
                                //var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                                var editPane = dataObjectsPane.items.map[scopeName + '.' + appName + '.editor-panel'];
                                var dbObjectsTree_SPPID = dataObjectsPane.items.items[0].items.items[0];
                                dbObjectsTree_SPPID.disable();
                                setTablesSelectorPane_SPPID(editPane, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane, dbTableNames_Stage);
                            },
                            failure: function (f, a) {
                                if (a.response) {
                                    var rtext = a.response.responseText;
                                    var error = 'SUCCESS = FALSE';
                                    var index = rtext.toUpperCase().indexOf(error);
                                    var msg = rtext.substring(index + error.length + 2, rtext.length - 1);
                                    showDialog(400, 100, 'Saving Result - Error', msg, Ext.Msg.OK, null);

                                }
                                else {
                                    showDialog(400, 100, 'Warning', 'Please fill in every field in this form.', Ext.Msg.OK, null);
                                }
                            },
                            waitMsg: 'Loading ...'
                        });
                    }
                }, {
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/edit-clear.png',
                    text: 'Reset',
                    tooltip: 'Reset to the latest applied changes',
                    handler: function (f) {
                        setDsConfigFields_SPPID(dsConfigPane_SPPID);
                    }
                }]
            })
        });

        if (SPPIDdbInfo) {
            setDsConfigFields_SPPID(dsConfigPane_SPPID, SPPIDdbInfo, dbDict_SPPID, dbPlantDict_SPPID);
        }
        editPane.add(dsConfigPane_SPPID);
        var panelIndex = editPane.items.indexOf(dsConfigPane_SPPID);
        editPane.getLayout().setActiveItem(panelIndex);
    }
};

function setAvailTables_SPPID(dbObjectsTree_SPPID, dbTableNames_Stage) {
    var availTableName = new Array();

    if (dbObjectsTree_SPPID.disabled) {
        for (var i = 0; i < dbTableNames_Stage.success.length; i++) {
            var tableName = dbTableNames_Stage.success[i];
            availTableName.push(tableName);
        }
    }
    else {
        var rootNode = dbObjectsTree_SPPID.getRootNode();
        if (dbTableNames_Stage.items) {
            for (var i = 0; i < dbTableNames_Stage.success.length; i++) {
                availTableName.push(dbTableNames_Stage.success[i]);
            }
        }

        if (!dbObjectsTree_SPPID.disabled) {
            for (var j = 0; j < availTableName.length; j++)
                for (var i = 0; i < rootNode.childNodes.length; i++) {
                    if (rootNode.childNodes[i].attributes.properties.tableName.toLowerCase() == availTableName[j].toLowerCase()) {
                        found = true;
                        availTableName.splice(j, 1);
                        j--;
                        break;
                    }
                }
        }
    }

    return availTableName;
}

function setSelectTables_SPPID(dbObjectsTree_SPPID, dbTableNames_Stage) {
    var selectTableNames = new Array();

    if (!dbObjectsTree_SPPID.disabled) {
        var rootNode = dbObjectsTree_SPPID.getRootNode();
        for (var i = 0; i < rootNode.childNodes.length; i++) {
            var nodeText = rootNode.childNodes[i].attributes.properties.tableName;
            selectTableNames.push([nodeText, nodeText]);
        }
    }

    return selectTableNames;
}


function setTablesSelectorPane_SPPID(editPane, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane, dbTableNames_Stage) {
    var dbObjectsTree_SPPID = dataObjectsPane.items.items[0].items.items[0];

    if (editPane) {
        if (editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane']) {
            var tableSelectorPanel = editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane'];
            if (tableSelectorPanel) {
                if (dbObjectsTree_SPPID.disabled)
                    tableSelectorPanel.destroy();
                else {
                    var panelIndex = editPane.items.indexOf(tableSelectorPanel);
                    editPane.getLayout().setActiveItem(panelIndex);
                    return;
                }
            }
        }

        var availItems_Stage = setAvailTables_SPPID(dbObjectsTree_SPPID, dbTableNames_Stage);
        var selectItems_Stage = setSelectTables_SPPID(dbObjectsTree_SPPID, dbTableNames_Stage);

        var tablesSelectorPane = new Ext.FormPanel({
            frame: false,
            border: false,
            autoScroll: true,
            id: scopeName + '.' + appName + '.tablesSelectorPane',
            bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
            labelWidth: 140,
            monitorValid: true,
            items: [{
                xtype: 'label',
                fieldLabel: 'Select Tables',
                labelSeparator: '',
                itemCls: 'form-title'
            }, {
                xtype: 'itemselector',
                hideLabel: true,
                bodyStyle: 'background:#eee',
                frame: true,
                name: 'tableSelector',
                imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
                multiselects: [{
                    width: 240,
                    height: 370,
                    store: availItems_Stage,
                    displayField: 'tableName',
                    valueField: 'tableValue',
                    border: 0
                }, {
                    width: 240,
                    height: 370,
                    store: selectItems_Stage,
                    displayField: 'tableName',
                    valueField: 'tableValue',
                    border: 0
                }],
                listeners: {
                    change: function (itemSelector, selectedValuesStr) {
                        var selectTables = itemSelector.toMultiselect.store.data.items;
                        for (var i = 0; i < selectTables.length; i++) {
                            var selectTableName = selectTables[i].data.text;
                            if (selectTableName == '')
                                itemSelector.toMultiselect.store.removeAt(i);
                        }

                        var availTables = itemSelector.fromMultiselect.store.data.items;
                        for (var i = 0; i < availTables.length; i++) {
                            var availTableName = availTables[i].data.text
                            if (availTables[i].data.text == '')
                                itemSelector.fromMultiselect.store.removeAt(i);
                        }
                    }
                }
            }, {
                xtype: 'checkbox',
                name: 'enableSummary',
                fieldLabel: 'Enable Summary'
            }],
            tbar: new Ext.Toolbar({
                items: [{
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/apply.png',
                    text: 'Apply',
                    tooltip: 'Apply the current changes to the data objects tree',
                    handler: function () {
                        //var tab = Ext.getCmp('content-panel');
                        //var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
                        //var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                        var dsConfigPane_SPPID = editPane.items.map[scopeName + '.' + appName + '.dsConfigPane_SPPID'];
                        var tablesSelectorPane = editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane'];
                        var tablesSelForm = tablesSelectorPane.getForm();
                        var dbObjectsTree_SPPID = dataObjectsPane.items.items[0].items.items[0];
                        var serName = '';
                        var serviceName = '';

                        if (dbObjectsTree_SPPID.disabled) {
                            dbObjectsTree_SPPID.enable();
                        }

                        if (dsConfigPane_SPPID) {
                            var serviceNamePane = dsConfigPane_SPPID.items.items[0].items.items[9];
                            if (serviceNamePane.items.items[0])
                                serName = serviceNamePane.items.items[0].serName;
                        }
                        else {
                            if (SPPIDdbInfo.serName)
                                serName = SPPIDdbInfo.serName;
                        }

                        var treeLoader = dbObjectsTree_SPPID.getLoader();
                        if (tablesSelForm.findField('tableSelector').getValue().indexOf('') == -1)
                            var selectTableNames = tablesSelForm.findField('tableSelector').getValue();
                        else {
                            var tableNames = tablesSelForm.findField('tableSelector').toMultiselect.store.data.items;
                            var selectTableNames = new Array();
                            for (var i = 0; i < tableNames.length; i++) {
                                selectTableNames.push(tableNames[i].data.text);
                            }
                        }

                        if (selectTableNames.length < 1) {
                            var rootNode = dbObjectsTree_SPPID.getRootNode();
                            while (rootNode.firstChild) {
                                rootNode.removeChild(rootNode.firstChild);
                            }
                            return;
                        }

                        userTableNames = new Array();

                        if (selectTableNames[1]) {
                            if (selectTableNames[1].length > 1 && selectTableNames[0].length > 1) {
                                for (var i = 0; i < selectTableNames.length; i++) {
                                    userTableNames.push(selectTableNames[i]);
                                }
                            }
                            else {
                                userTableNames.push(selectTableNames)
                            }
                        }
                        else {
                            userTableNames.push(selectTableNames[0]);
                        }

                        treeLoader.dataUrl = 'SPPID/DBObjects';
                        if (dsConfigPane_SPPID) {
                            var dsConfigForm = dsConfigPane_SPPID.getForm();
                            treeLoader.baseParams = {
                                scope: scopeName,
                                app: appName,
                                dbProvider: dsConfigForm.findField('dbstageProvider').getValue(),
                                dbServer: dsConfigForm.findField('dbstageServer').getValue(),
                                dbInstance: dsConfigForm.findField('dbstageInstance').getValue(),
                                dbName: dsConfigForm.findField('dbstageName').getValue(),
                                dbSchema: 'dbo',
                                dbUserName: dsConfigForm.findField('dbstageUserName').getValue(),
                                dbPassword: dsConfigForm.findField('dbstagePassword').getValue(),
                                portNumber: dsConfigForm.findField('stageportNumber').getValue(),
                                tableNames: selectTableNames,
                                serName: serName
                            };
                        }
                        else {
                            treeLoader.baseParams = {
                                scope: scopeName,
                                app: appName,
                                dbProvider: dbDict_SPPID.Provider,
                                dbServer: SPPIDdbInfo.dbServer,
                                dbInstance: SPPIDdbInfo.dbInstance,
                                dbName: SPPIDdbInfo.dbName,
                                dbSchema: dbDict_SPPID.SchemaName,
                                dbUserName: SPPIDdbInfo.dbUserName,
                                dbPassword: SPPIDdbInfo.dbPassword,
                                portNumber: SPPIDdbInfo.portNumber,
                                tableNames: selectTableNames,
                                serName: serName
                            };
                        }

                        treeLoader.on('beforeload', function (treeLoader, node) {
                            dataObjectsPane.body.mask('Loading...', 'x-mask-loading');
                        }, this);

                        treeLoader.on('load', function (treeLoader, node) {
                            dataObjectsPane.body.unmask();
                        }, this);

                        var rootNode = dbObjectsTree_SPPID.getRootNode();
                        rootNode.reload(
              function (rootNode) {
                  loadTree_SPPID(rootNode, dbDict_SPPID);
              });
                    }
                }, {
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/edit-clear.png',
                    text: 'Reset',
                    tooltip: 'Reset to the latest applied changes',
                    handler: function () {
                        var rootNode = dataObjectsPane.items.items[0].items.items[0].getRootNode();
                        var selectTableNames = new Array();
                        var selectTableNamesSingle = new Array();
                        var firstSelectTableNames = new Array();
                        var availTableName = new Array();
                        var found = false;

                        for (var i = 0; i < dbTableNames_Stage.items.length; i++) {
                            availTableName.push(dbTableNames_Stage.items[i]);
                        }

                        for (var j = 0; j < availTableName.length; j++)
                            for (var i = 0; i < rootNode.childNodes.length; i++) {
                                if (rootNode.childNodes[i].attributes.properties.tableName.toLowerCase() == availTableName[j].toLowerCase()) {
                                    found = true;
                                    availTableName.splice(j, 1);
                                    j--;
                                    break;
                                }
                            }

                        for (var i = 0; i < rootNode.childNodes.length; i++) {
                            var nodeText = rootNode.childNodes[i].attributes.properties.tableName;
                            selectTableNames.push([nodeText, nodeText]);
                            selectTableNamesSingle.push(nodeText);
                        }

                        if (selectTableNames[0]) {
                            firstSelectTableNames.push(selectTableNames[0]);
                            var tablesSelector = tablesSelectorPane.items.items[1];

                            if (tablesSelector.toMultiselect.store.data) {
                                tablesSelector.toMultiselect.reset();
                                tablesSelector.toMultiselect.store.removeAll();
                            }

                            tablesSelector.toMultiselect.store.loadData(firstSelectTableNames);
                            tablesSelector.toMultiselect.store.commitChanges();

                            var firstSelectTables = tablesSelector.toMultiselect.store.data.items;
                            var loadSingle = false;
                            var selectTableName = firstSelectTables[0].data.text;

                            if (selectTableName[1])
                                if (selectTableName[1].length > 1)
                                    var loadSingle = true;

                            tablesSelector.toMultiselect.reset();
                            tablesSelector.toMultiselect.store.removeAll();

                            if (!loadSingle)
                                tablesSelector.toMultiselect.store.loadData(selectTableNames);
                            else
                                tablesSelector.toMultiselect.store.loadData(selectTableNamesSingle);

                            tablesSelector.toMultiselect.store.commitChanges();
                        }
                        else {
                            if (!tablesSelector)
                                var tablesSelector = tablesSelectorPane.items.items[1];
                            if (tablesSelector.toMultiselect) {
                                tablesSelector.toMultiselect.reset();
                                tablesSelector.toMultiselect.store.removeAll();
                                tablesSelector.toMultiselect.store.commitChanges();
                            }
                        }

                        if (tablesSelector.fromMultiselect.store.data) {
                            tablesSelector.fromMultiselect.reset();
                            tablesSelector.fromMultiselect.store.removeAll();
                        }

                        tablesSelector.fromMultiselect.store.loadData(availTableName);
                        tablesSelector.fromMultiselect.store.commitChanges();
                    }
                }]
            })
        });
        editPane.add(tablesSelectorPane);
        var panelIndex = editPane.items.indexOf(tablesSelectorPane);
        editPane.getLayout().setActiveItem(panelIndex);

    }
};


function loadTree_SPPID(rootNode, dbDict_SPPID) {
    var shownProperty = new Array();
    var relationTypeStr = ['OneToOne', 'OneToMany'];

    // sync data object tree with data dictionary
    for (var i = 0; i < rootNode.childNodes.length; i++) {
        var dataObjectNode = rootNode.childNodes[i];
        dataObjectNode.attributes.properties.tableName = dataObjectNode.text;
        for (var ijk = 0; ijk < dbDict_SPPID.dataObjects.length; ijk++) {
            var dataObject = dbDict_SPPID.dataObjects[ijk];

            if (dataObjectNode.text.toUpperCase() != dataObject.tableName.toUpperCase())
                continue;

            // sync data object
            dataObjectNode.attributes.properties.objectNamespace = dataObject.objectNamespace;
            dataObjectNode.attributes.properties.objectName = dataObject.objectName;
            dataObjectNode.attributes.properties.keyDelimiter = dataObject.keyDelimeter;
            dataObjectNode.attributes.properties.description = dataObject.description;
            dataObjectNode.text = dataObject.objectName;
            dataObjectNode.attributes.text = dataObject.objectName;
            dataObjectNode.setText(dataObject.objectName);

            if (dataObject.objectName.toLowerCase() == dataObjectNode.text.toLowerCase()) {
                var keysNode = dataObjectNode.attributes.children[0];
                var propertiesNode = dataObjectNode.attributes.children[1];
                var relationshipsNode = dataObjectNode.attributes.children[2];

                // sync data properties
                for (var j = 0; j < propertiesNode.children.length; j++) {
                    for (var jj = 0; jj < dataObject.dataProperties.length; jj++) {
                        if (propertiesNode.children[j].text.toLowerCase() == dataObject.dataProperties[jj].columnName.toLowerCase()) {

                            if (!hasShown(shownProperty, propertiesNode.children[j].text.toLowerCase())) {
                                shownProperty.push(propertiesNode.children[j].text.toLowerCase());
                                propertiesNode.children[j].hidden = false;
                            }

                            propertiesNode.children[j].text = dataObject.dataProperties[jj].propertyName;
                            propertiesNode.children[j].properties.propertyName = dataObject.dataProperties[jj].propertyName;
                            propertiesNode.children[j].properties.isHidden = dataObject.dataProperties[jj].isHidden;
                        }
                    }
                }

                // sync key properties
                for (var ij = 0; ij < dataObject.keyProperties.length; ij++) {
                    for (var k = 0; k < keysNode.children.length; k++) {
                        for (var ikk = 0; ikk < dataObject.dataProperties.length; ikk++) {
                            if (dataObject.keyProperties[ij].keyPropertyName.toLowerCase() == dataObject.dataProperties[ikk].propertyName.toLowerCase()) {
                                if (keysNode.children[k].text.toLowerCase() == dataObject.dataProperties[ikk].columnName.toLowerCase()) {
                                    keysNode.children[k].text = dataObject.keyProperties[ij].keyPropertyName;
                                    keysNode.children[k].properties.propertyName = dataObject.keyProperties[ij].keyPropertyName;
                                    keysNode.children[k].properties.isHidden = dataObject.keyProperties[ij].isHidden;
                                    ij++;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    if (ij < dataObject.keyProperties.length) {
                        for (var ijj = 0; ijj < propertiesNode.children.length; ijj++) {
                            var nodeText = dataObject.keyProperties[ij].keyPropertyName;
                            if (propertiesNode.children[ijj].text.toLowerCase() == nodeText.toLowerCase()) {
                                var properties = propertiesNode.children[ijj].properties;
                                properties.propertyName = nodeText;
                                //properties.keyType = 'assigned';
                                //properties.nullable = false;

                                newKeyNode = new Ext.tree.TreeNode({
                                    text: nodeText,
                                    type: "keyProperty",
                                    leaf: true,
                                    iconCls: 'treeKey',
                                    hidden: false,
                                    properties: properties
                                });
                                newKeyNode.iconCls = 'treeKey';
                                propertiesNode.children.splice(ijj, 1);
                                ijj--;

                                if (newKeyNode)
                                    keysNode.children.push(newKeyNode);

                                break;
                            }
                        }
                    }
                }

                // sync relationships
                for (var kj = 0; kj < dataObject.dataRelationships.length; kj++) {
                    var newNode = new Ext.tree.TreeNode({
                        text: dataObject.dataRelationships[kj].relationshipName,
                        type: 'relationship',
                        leaf: true,
                        iconCls: 'treeRelation',
                        relatedObjMap: [],
                        objectName: dataObjectNode.text,
                        relatedObjectName: dataObject.dataRelationships[kj].relatedObjectName,
                        relationshipType: relationTypeStr[dataObject.dataRelationships[kj].relationshipType],
                        relationshipTypeIndex: dataObject.dataRelationships[kj].relationshipType,
                        propertyMap: []
                    });
                    var mapArray = new Array();
                    for (var kjj = 0; kjj < dataObject.dataRelationships[kj].propertyMaps.length; kjj++) {
                        var mapItem = new Array();
                        mapItem['dataPropertyName'] = dataObject.dataRelationships[kj].propertyMaps[kjj].dataPropertyName;
                        mapItem['relatedPropertyName'] = dataObject.dataRelationships[kj].propertyMaps[kjj].relatedPropertyName;
                        mapArray.push(mapItem);
                    }
                    newNode.iconCls = 'treeRelation';
                    newNode.attributes.propertyMap = mapArray;
                    relationshipsNode.expanded = true;
                    relationshipsNode.children.push(newNode);
                }
            }
        }
        ijk++;
    }
};

function setTableNames(dbDict_SPPID) {
    // populate selected tables			
    var selectTableNames = new Array();

    for (var i = 0; i < dbDict_SPPID.dataObjects.length; i++) {
        var tableName = (dbDict_SPPID.dataObjects[i].tableName ? dbDict_SPPID.dataObjects[i].tableName : dbDict_SPPID.dataObjects[i]);
        selectTableNames.push(tableName);
    }

    return selectTableNames;
};

function showTree_SPPID(dbObjectsTree_SPPID, SPPIDdbInfo, dbDict_SPPID, scopeName, appName, dataObjectsPane) {

    var selectTableNames = setTableNames(dbDict_SPPID);
    var connStr = dbDict_SPPID.ConnectionString;
    if (!connStr) {
        showDialog(400, 100, 'Warning', 'Please save the Data Objects tree first.', Ext.Msg.OK, null);
    }

    var connStrParts = connStr.split(';');
    SPPIDdbInfo = {};
    var provider = dbDict_SPPID.Provider.toUpperCase();

    SPPIDdbInfo.dbName = dbDict_SPPID.SchemaName;
    if (!SPPIDdbInfo.dbUserName)
        for (var i = 0; i < connStrParts.length; i++) {
            var pair = connStrParts[i].split('=');
            switch (pair[0].toUpperCase()) {
                case 'DATA SOURCE':
                    if (provider.indexOf('MSSQL') > -1) {
                        var dsValue = pair[1].split('\\');
                        SPPIDdbInfo.dbServer = (dsValue[0].toLowerCase() == '.' ? 'localhost' : dsValue[0]);
                        SPPIDdbInfo.dbInstance = dsValue[1];
                        SPPIDdbInfo.portNumber = 1433;
                        SPPIDdbInfo.serName = '';
                    }
                    else if (provider.indexOf('MYSQL') > -1) {
                        SPPIDdbInfo.dbServer = (pair[1].toLowerCase() == '.' ? 'localhost' : pair[1]);
                        SPPIDdbInfo.portNumber = 3306;
                    }
                    else if (provider.indexOf('ORACLE') > -1) {
                        var dsStr = connStrParts[i].substring(12, connStrParts[i].length);
                        var dsValue = dsStr.split('=');
                        for (var j = 0; j < dsValue.length; j++) {
                            dsValue[j] = dsValue[j].substring(dsValue[j].indexOf('(') + 1, dsValue[j].length);
                            switch (dsValue[j].toUpperCase()) {
                                case 'HOST':
                                    var server = dsValue[j + 1];
                                    var port = dsValue[j + 2];
                                    var index = server.indexOf(')');
                                    server = server.substring(0, index);
                                    SPPIDdbInfo.portNumber = port.substring(0, 4);
                                    SPPIDdbInfo.dbServer = (server.toLowerCase() == '.' ? 'localhost' : server);
                                    break;
                                case 'SERVICE_NAME':
                                    var sername = dsValue[j + 1];
                                    index = sername.indexOf(')');
                                    SPPIDdbInfo.dbInstance = sername.substring(0, index);
                                    SPPIDdbInfo.serName = 'SERVICE_NAME';
                                    break;
                                case 'SID':
                                    var sername = dsValue[j + 1];
                                    index = sername.indexOf(')');
                                    SPPIDdbInfo.dbInstance = sername.substring(0, index);
                                    SPPIDdbInfo.serName = 'SID';
                                    break;
                            }
                        }
                    }
                    break;
                case 'INITIAL CATALOG':
                    SPPIDdbInfo.dbName = pair[1];
                    break;
                case 'USER ID':
                    SPPIDdbInfo.dbUserName = pair[1];
                    break;
                case 'PASSWORD':
                    SPPIDdbInfo.dbPassword = pair[1];
                    break;
            }
        }

    var treeLoader = dbObjectsTree_SPPID.getLoader();
    var rootNode = dbObjectsTree_SPPID.getRootNode();

    treeLoader.dataUrl = 'AdapterManager/DBObjects';
    treeLoader.baseParams = {
        scope: scopeName,
        app: appName,
        dbProvider: dbDict_SPPID.Provider,
        dbServer: SPPIDdbInfo.dbServer,
        dbInstance: SPPIDdbInfo.dbInstance,
        dbName: SPPIDdbInfo.dbName,
        dbSchema: dbDict_SPPID.SchemaName,
        dbUserName: SPPIDdbInfo.dbUserName,
        dbPassword: SPPIDdbInfo.dbPassword,
        portNumber: SPPIDdbInfo.portNumber,
        tableNames: selectTableNames,
        serName: SPPIDdbInfo.serName
    };

    treeLoader.on('beforeload', function (treeLoader, node) {
        dataObjectsPane.body.mask('Loading...', 'x-mask-loading');
    }, this);

    treeLoader.on('load', function (treeLoader, node) {
        dataObjectsPane.body.unmask();
    }, this);

    rootNode.reload(
      function (rootNode) {
          loadTree(rootNode, dbDict_SPPID);
      });

    Ext.Ajax.request({
        url: 'AdapterManager/TableNames',
        timeout: 600000,
        method: 'POST',
        params: {
            scope: scopeName,
            app: appName,
            dbProvider: dbDict_SPPID.Provider,
            dbServer: SPPIDdbInfo.dbServer,
            dbInstance: SPPIDdbInfo.dbInstance,
            dbName: SPPIDdbInfo.dbName,
            dbSchema: dbDict_SPPID.SchemaName,
            dbUserName: SPPIDdbInfo.dbUserName,
            dbPassword: SPPIDdbInfo.dbPassword,
            portNumber: SPPIDdbInfo.portNumber,
            serName: SPPIDdbInfo.serName
        },
        success: function (response, request) {
            SPPIDdbInfo.dbTableNames_Stage = Ext.util.JSON.decode(response.responseText);
        },
        failure: function (f, a) {
            if (a.response)
                showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
        }
    });
    return SPPIDdbInfo;
};

function setDataProperty_SPPID(editPane, node, scopeName, appName, dataTypes_SPPID) {
    if (editPane && node) {
        if (editPane.items.map[scopeName + '.' + appName + '.dataPropertyForm.' + node.id]) {
            var dataPropertyFormPane = editPane.items.map[scopeName + '.' + appName + '.dataPropertyForm.' + node.id];
            if (dataPropertyFormPane) {
                var panelIndex = editPane.items.indexOf(dataPropertyFormPane);
                editPane.getLayout().setActiveItem(panelIndex);
                return;
            }
        }

        var dataPropertyFormPanel = new Ext.FormPanel({
            name: 'dataProperty',
            id: scopeName + '.' + appName + '.dataPropertyForm.' + node.id,
            border: false,
            autoScroll: true,
            monitorValid: true,
            labelWidth: 130,
            bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
            defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
            items: [{
                xtype: 'label',
                fieldLabel: 'Data Properties',
                labelSeparator: '',
                itemCls: 'form-title'
            }, {
                name: 'columnName',
                fieldLabel: 'Column Name',
                disabled: true
            }, {
                name: 'propertyName',
                fieldLabel: 'Property Name'
            }, {
                name: 'dataType',
                xtype: 'combo',
                fieldLabel: 'Data Type',
                store: dataTypes_SPPID,
                mode: 'local',
                editable: false,
                triggerAction: 'all',
                displayField: 'text',
                valueField: 'value',
                selectOnFocus: true,
                disabled: true
            }, {
                xtype: 'numberfield',
                name: 'dataLength',
                fieldLabel: 'Data Length'
            }, {
                xtype: 'checkbox',
                name: 'isNullable',
                fieldLabel: 'Nullable',
                disabled: true
            }, {
                xtype: 'checkbox',
                name: 'showOnIndex',
                fieldLabel: 'Show on Index'
            }, {
                xtype: 'numberfield',
                name: 'numberOfDecimals',
                fieldLabel: 'Number of Decimals'
            }, {
                xtype: 'checkbox',
                name: 'isHidden',
                fieldLabel: 'Hidden'
            }],
            treeNode: node,
            tbar: new Ext.Toolbar({
                items: [{
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/apply.png',
                    text: 'Apply',
                    tooltip: 'Apply the current changes to the data objects tree',
                    handler: function (f) {
                        var form = dataPropertyFormPanel.getForm();
                        if (form.treeNode)
                            applyProperty(form);
                    }
                }, {
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/edit-clear.png',
                    text: 'Reset',
                    tooltip: 'Reset to the latest applied changes',
                    handler: function (f) {
                        var form = dataPropertyFormPanel.getForm();
                        setDataPropertyFields_SPPID(form, node.attributes.properties);
                    }
                }]
            })
        });
        var form = dataPropertyFormPanel.getForm();
        setDataPropertyFields_SPPID(form, node.attributes.properties);
        editPane.add(dataPropertyFormPanel);
        var panelIndex = editPane.items.indexOf(dataPropertyFormPanel);
        editPane.getLayout().setActiveItem(panelIndex);
    }
}

function setDataPropertyFields_SPPID(form, properties) {
    if (form && properties) {
        form.findField('columnName').setValue(properties.columnName);
        form.findField('propertyName').setValue(properties.propertyName);
        form.findField('dataType').setValue(properties.dataType);
        form.findField('dataLength').setValue(properties.dataLength);

        if (properties.nullable)
            if (properties.nullable.toString().toLowerCase() == 'true') {
                form.findField('isNullable').setValue(true);
            }
            else {
                form.findField('isNullable').setValue(false);
            }
        else
            form.findField('isNullable').setValue(false);

        if (properties.showOnIndex.toString().toLowerCase() == 'true') {
            form.findField('showOnIndex').setValue(true);
        }
        else {
            form.findField('showOnIndex').setValue(false);
        }

        if (properties.isHidden.toString().toLowerCase() == 'true') {
            form.findField('isHidden').setValue(true);
        }
        else {
            form.findField('isHidden').setValue(false);
        }

        form.findField('numberOfDecimals').setValue(properties.numberOfDecimals);
    }
}

function setDataObject_SPPID(editPane, node, dbDict_SPPID, dataObjectsPane, scopeName, appName) {
    if (editPane && node) {
        if (editPane.items.map[scopeName + '.' + appName + '.objectNameForm.' + node.id]) {
            var objectNameFormPane = editPane.items.map[scopeName + '.' + appName + '.objectNameForm.' + node.id];
            if (objectNameFormPane) {
                var panelIndex = editPane.items.indexOf(objectNameFormPane);
                editPane.getLayout().setActiveItem(panelIndex);
                return;
            }
        }

        if (!node.attributes.properties.objectNamespace)
            node.attributes.properties.objectNamespace = "org.iringtools.adapter.datalayer.proj_" + scopeName + "." + appName;

        if (node.attributes.properties.keyDelimiter == null || !node.attributes.properties.keyDelimiter || node.attributes.properties.keyDelimiter == "null")
            node.attributes.properties.keyDelimiter = '_';

        var dataObjectFormPanel = new Ext.FormPanel({
            name: 'dataObject',
            id: scopeName + '.' + appName + '.objectNameForm.' + node.id,
            border: false,
            autoScroll: true,
            monitorValid: true,
            labelWidth: 160,
            bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
            defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
            items: [{
                xtype: 'label',
                fieldLabel: 'Data Object',
                labelSeparator: '',
                itemCls: 'form-title'
            }, {
                name: 'tableName',
                fieldLabel: 'Table Name',
                value: node.attributes.properties.tableName,
                disabled: true
            }, {
                name: 'objectNamespace',
                fieldLabel: 'Object Namespace',
                value: node.attributes.properties.objectNamespace
            }, {
                name: 'objectName',
                fieldLabel: 'Object Name',
                value: node.attributes.properties.objectName
            }, {
                name: 'keyDelimeter',
                fieldLabel: 'Key Delimiter',
                value: node.attributes.properties.keyDelimiter,
                allowBlank: true
            }, {
                name: 'description',
                xtype: 'textarea',
                height: 150,
                fieldLabel: 'Description',
                value: node.attributes.properties.description,
                allowBlank: true
            }],
            treeNode: node,
            tbar: new Ext.Toolbar({
                items: [{
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/apply.png',
                    text: 'Apply',
                    tooltip: 'Apply the current changes to the data objects tree',
                    handler: function (f) {
                        var form = dataObjectFormPanel.getForm();
                        if (form.treeNode) {
                            var treeNodeProps = form.treeNode.attributes.properties;
                            var objNam = form.findField('objectName').getValue();
                            var oldObjNam = treeNodeProps['objectName'];
                            treeNodeProps['tableName'] = form.findField('tableName').getValue();
                            treeNodeProps['objectName'] = objNam;
                            treeNodeProps['keyDelimiter'] = form.findField('keyDelimeter').getValue();
                            treeNodeProps['description'] = form.findField('description').getValue();

                            for (var ijk = 0; ijk < dbDict_SPPID.dataObjects.length; ijk++) {
                                var dataObject = dbDict_SPPID.dataObjects[ijk];
                                if (form.treeNode.text.toUpperCase() != dataObject.objectName.toUpperCase())
                                    continue;
                                dataObject.objectName = objNam;
                            }

                            form.treeNode.setText(objNam);
                            form.treeNode.text = objNam;
                            form.treeNode.attributes.text = objNam;
                            form.treeNode.attributes.properties.objectName = objNam;

                            var dsConfigPane_SPPID = editPane.items.map[scopeName + '.' + appName + '.dsConfigPane_SPPID'];
                            var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
                            var rootNode = dbObjectsTree.getRootNode();

                            for (var i = 0; i < rootNode.childNodes.length; i++) {
                                var folderNode = rootNode.childNodes[i];
                                var folderNodeProp = folderNode.attributes.properties;
                                if (folderNode.childNodes[2])
                                    var relationFolderNode = folderNode.childNodes[2];
                                else
                                    var relationFolderNode = folderNode.attributes.children[2];

                                if (!relationFolderNode)
                                    continue;

                                if (relationFolderNode.childNodes)
                                    var relChildenNodes = relationFolderNode.childNodes;
                                else
                                    var relChildenNodes = relationFolderNode.children;

                                if (relChildenNodes) {
                                    for (var k = 0; k < relChildenNodes.length; k++) {
                                        var relationNode = relChildenNodes[k];

                                        if (relationNode.text == '')
                                            continue;

                                        if (relationNode.attributes.attributes)
                                            var relationNodeAttr = relationNode.attributes.attributes;
                                        else
                                            var relationNodeAttr = relationNode.attributes;

                                        var relObjNam = relationNodeAttr.relatedObjectName;
                                        if (relObjNam.toLowerCase() != objNam.toLowerCase() && relObjNam.toLowerCase() == oldObjNam.toLowerCase())
                                            relationNodeAttr.relatedObjectName = objNam;

                                        var relatedObjPropMap = relationNodeAttr.relatedObjMap;

                                        for (var iki = 0; iki < relatedObjPropMap.length; iki++) {
                                            if (relatedObjPropMap[iki].relatedObjName.toLowerCase() == oldObjNam.toLowerCase())
                                                relatedObjPropMap[iki].relatedObjName = objNam;
                                        }
                                    }
                                }
                            }

                            var items = editPane.items.items;

                            for (var i = 0; i < items.length; i++) {
                                var relateObjField = items[i].getForm().findField('relatedObjectName');
                                if (relateObjField)
                                    if (relateObjField.getValue().toLowerCase() == oldObjNam.toLowerCase())
                                        relateObjField.setValue(objNam);
                            }
                        }
                    }
                }, {
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/edit-clear.png',
                    text: 'Reset',
                    tooltip: 'Reset to the latest applied changes',
                    handler: function (f) {
                        var form = dataObjectFormPanel.getForm();
                        if (node.attributes.properties) {
                            form.findField('objectName').setValue(node.attributes.properties.objectName);
                            form.findField('keyDelimeter').setValue(node.attributes.properties.keyDelimiter);
                            form.findField('description').setValue(node.attributes.properties.description);
                        }
                    }
                }]
            })
        });
        editPane.add(dataObjectFormPanel);
        var panelIndex = editPane.items.indexOf(dataObjectFormPanel);
        editPane.getLayout().setActiveItem(panelIndex);
    }
};

function getTreeJson_SPPID(dsConfigPane_SPPID, rootNode, SPPIDdbInfo, dbDict_SPPID, dataTypes_SPPID, tablesSelectorPane) {
    var treeProperty = {};
    treeProperty.dataObjects = new Array();
    treeProperty.IdentityConfiguration = null;

    var tProp = setTreeProperty_SPPID(dsConfigPane_SPPID, SPPIDdbInfo, dbDict_SPPID, tablesSelectorPane);
    treeProperty.connectionString = tProp.connectionString;
    if (treeProperty.connectionString != null && treeProperty.connectionString.length > 0) {
        treeProperty.connectionString = Base64.encode(tProp.connectionString);
    }
    treeProperty.schemaName = tProp.schemaName;
    treeProperty.provider = tProp.provider;
    treeProperty.enableSummary = tProp.enableSummary;

    var keyName;
    for (var i = 0; i < rootNode.childNodes.length; i++) {
        var folder = getFolderFromChildNode_SPPID(rootNode.childNodes[i], dataTypes_SPPID);
        treeProperty.dataObjects.push(folder);
    }

    dbDict_SPPID.ConnectionString = treeProperty.connectionString;
    dbDict_SPPID.SchemaName = treeProperty.schemaName;
    dbDict_SPPID.Provider = treeProperty.provider;
    dbDict_SPPID.dataObjects = treeProperty.dataObjects;

    return treeProperty;
};
function setTreeProperty_SPPID(dsConfigPane_SPPID, dbInfo, dbDict, tablesSelectorPane) {
    var treeProperty = {};
    if (tablesSelectorPane)
        treeProperty.enableSummary = tablesSelectorPane.getForm().findField('enableSummary').getValue();
    else if (dbDict.enableSummary)
        treeProperty.enableSummary = dbDict.enableSummary;
    else
        treeProperty.enableSummary = false;

    if (dsConfigPane_SPPID) {
        var dsConfigForm = dsConfigPane_SPPID.getForm();
        treeProperty.provider = dsConfigForm.findField('dbstageProvider').getValue();
        var dbServer = dsConfigForm.findField('dbstageServer').getValue();
        dbServer = (dbServer.toLowerCase() == 'localhost' ? '.' : dbServer);
        var upProvider = treeProperty.provider.toUpperCase();
        /*        var serviceNamePane = dsConfigPane_SPPID.items.items[9].items.items[9];
        var serviceName = '';
        var serName = '';
        if (serviceNamePane.items.items[0]) {
        serviceName = serviceNamePane.items.items[0].value;
        serName = serviceNamePane.items.items[0].serName;
        }
        else if (dbInfo) {
        if (dbInfo.dbInstance)
        serviceName = dbInfo.dbInstance;
        if (dbInfo.serName)
        serName = dbInfo.serName;
        } */

        if (upProvider.indexOf('MSSQL') > -1) {
            var dbInstance = dsConfigForm.findField('dbstageInstance').getValue();
            var dbDatabase = dsConfigForm.findField('dbstageName').getValue();
            if (dbInstance.toUpperCase() == "DEFAULT") {
                var dataSrc = 'Data Source=' + dbServer + ';Initial Catalog=' + dbDatabase;
            } else {
                var dataSrc = 'Data Source=' + dbServer + '\\' + dbInstance + ';Initial Catalog=' + dbDatabase;
            }
        }
        /*    else if (upProvider.indexOf('ORACLE') > -1)
        var dataSrc = 'Data Source=' + '(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=' + dbServer + ')(PORT=' + dsConfigForm.findField('portNumber').getValue() + ')))(CONNECT_DATA=(SERVER=DEDICATED)(' + serName + '=' + serviceName + ')))';
        else if (upProvider.indexOf('MYSQL') > -1)
        var dataSrc = 'Data Source=' + dbServer; */

        treeProperty.connectionString = dataSrc
                                  + ';User ID=' + dsConfigForm.findField('dbstageUserName').getValue()
                                  + ';Password=' + dsConfigForm.findField('dbstagePassword').getValue();

        treeProperty.schemaName = 'dbo';
    }
    else {
        treeProperty.provider = dbDict.Provider;
        var dbServer = dbInfo.dbServer;
        var upProvider = treeProperty.provider.toUpperCase();
        dbServer = (dbServer.toLowerCase() == 'localhost' ? '.' : dbstageServer);

        if (upProvider.indexOf('MSSQL') > -1) {
            if (dbInfo.dbInstance) {
                if (dbInfo.dbInstance.toUpperCase() == "DEFAULT") {
                    var dataSrc = 'Data Source=' + dbstageServer + ';Initial Catalog=' + dbInfo.dbName;
                } else {
                    var dataSrc = 'Data Source='
					            + dbstageServer + '\\' + dbInfo.dbInstance
											+ ';Initial Catalog=' + dbInfo.dbName;
                }
            }
        }
        /*  else if (upProvider.indexOf('ORACLE') > -1)
        var dataSrc = 'Data Source=' + '(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=' + dbServer + ')(PORT=' + dbInfo.portNumber + ')))(CONNECT_DATA=(SERVER=DEDICATED)(' + dbInfo.serName + '=' + dbInfo.dbInstance + ')))';
        else if (upProvider.indexOf('MYSQL') > -1)
        var dataSrc = 'Data Source=' + dbServer;

        treeProperty.connectionString = dataSrc
        + ';User ID=' + dbInfo.dbUserName
        + ';Password=' + dbInfo.dbPassword;
        treeProperty.schemaName = dbDict.SchemaName; */
    }
    return treeProperty;
};

function getDataTypeIndex_SPPID(datatype, dataTypes_SPPID) {
    var i = 0;

    while (!dataTypes_SPPID[i])
        i++;

    for (var k = i; k < dataTypes_SPPID.length; k++) {
        if (dataTypes_SPPID[k][1] == datatype)
            return dataTypes_SPPID[k][0];
    }
};

function getFolderFromChildNode_SPPID(folderNode, dataTypes_SPPID) {
    var folderNodeProp = folderNode.attributes.properties;
    var folder = {};
    var keyName = '';

    folder.tableName = folderNodeProp.tableName;
    folder.objectNamespace = folderNodeProp.objectNamespace;
    folder.objectName = folderNodeProp.objectName;
    folder.description = folderNodeProp.description;

    if (!folderNodeProp.keyDelimiter)
        folder.keyDelimeter = 'null';
    else
        folder.keyDelimeter = folderNodeProp.keyDelimiter;

    folder.keyProperties = new Array();
    folder.dataProperties = new Array();
    folder.dataRelationships = new Array();

    for (var j = 0; j < folderNode.attributes.children.length; j++) {
        if (folderNode.childNodes[1])
            var propertyFolderNode = folderNode.childNodes[1];
        else
            var propertyFolderNode = folderNode.attributes.children[1];

        if (folderNode.childNodes[0])
            var keyFolderNode = folderNode.childNodes[0];
        else
            var keyFolderNode = folderNode.attributes.children[0];

        if (folderNode.childNodes[2])
            var relationFolderNode = folderNode.childNodes[2];
        else
            var relationFolderNode = folderNode.attributes.children[2];

        if (folderNode.childNodes[j])
            subFolderNodeText = folderNode.childNodes[j].text;
        else
            subFolderNodeText = folderNode.attributes.children[j].text;

        switch (subFolderNodeText) {
            case 'Keys':
                if (folderNode.childNodes[1])
                    var keyChildenNodes = keyFolderNode.childNodes;
                else
                    var keyChildenNodes = keyFolderNode.children;

                for (var k = 0; k < keyChildenNodes.length; k++) {
                    var keyNode = keyChildenNodes[k];

                    if (!keyNode.hidden) {
                        var keyProps = {};

                        if (keyNode.properties)
                            var keyNodeProf = keyNode.properties;
                        else if (keyNode.attributes.attributes)
                            var keyNodeProf = keyNode.attributes.attributes.properties;
                        else
                            var keyNodeProf = keyNode.attributes.properties;

                        keyProps.keyPropertyName = keyNode.text;
                        keyName = keyNode.text;
                        folder.keyProperties.push(keyProps);

                        var tagProps = {};
                        tagProps.columnName = keyNodeProf.columnName;
                        tagProps.propertyName = keyNode.text;
                        if (typeof keyNodeProf.dataType == 'string')
                            tagProps.dataType = getDataTypeIndex_SPPID(keyNodeProf.dataType, dataTypes_SPPID);
                        else
                            tagProps.dataType = keyNodeProf.dataType;

                        tagProps.dataLength = keyNodeProf.dataLength;

                        if (keyNodeProf.nullable)
                            tagProps.isNullable = keyNodeProf.nullable.toString().toLowerCase();
                        else
                            tagProps.isNullable = 'false';

                        if (keyNodeProf.isHidden)
                            tagProps.isHidden = keyNodeProf.isHidden.toString().toLowerCase();
                        else
                            tagProps.isHidden = 'false';

                        if (!keyNodeProf.keyType)
                            tagProps.keyType = 1;
                        else
                            if (typeof keyNodeProf.keyType != 'string')
                                tagProps.keyType = keyNodeProf.keyType;
                            else {
                                switch (keyNodeProf.keyType.toLowerCase()) {
                                    case 'assigned':
                                        tagProps.keyType = 1;
                                        break;
                                    case 'unassigned':
                                        tagProps.keyType = 0;
                                        break;
                                    default:
                                        tagProps.keyType = 1;
                                        break;
                                }
                            }

                        if (keyNodeProf.showOnIndex)
                            tagProps.showOnIndex = keyNodeProf.showOnIndex.toString().toLowerCase();
                        else
                            tagProps.showOnIndex = 'false';

                        tagProps.numberOfDecimals = keyNodeProf.numberOfDecimals;
                        folder.dataProperties.push(tagProps);
                    }
                }
                break;
            case 'Properties':
                if (folderNode.childNodes[1])
                    var propChildenNodes = propertyFolderNode.childNodes;
                else
                    var propChildenNodes = propertyFolderNode.children;
                for (var k = 0; k < propChildenNodes.length; k++) {
                    var propertyNode = propChildenNodes[k];

                    if (!propertyNode.hidden) {
                        if (propertyNode.properties)
                            var propertyNodeProf = propertyNode.properties;
                        else if (propertyNode.attributes)
                            var propertyNodeProf = propertyNode.attributes.properties;

                        var props = {};
                        props.columnName = propertyNodeProf.columnName;
                        props.propertyName = propertyNodeProf.propertyName;

                        if (typeof propertyNodeProf.dataType == 'string')
                            props.dataType = getDataTypeIndex_SPPID(propertyNodeProf.dataType, dataTypes_SPPID);
                        else
                            props.dataType = propertyNodeProf.dataType;

                        props.dataLength = propertyNodeProf.dataLength;

                        if (propertyNodeProf.nullable)
                            props.isNullable = propertyNodeProf.nullable.toString().toLowerCase();
                        else
                            props.isNullable = 'false';

                        if (keyName != '') {
                            if (props.columnName == keyName)
                                props.keyType = 1;
                            else
                                props.keyType = 0;
                        }
                        else
                            props.keyType = 0;

                        if (propertyNodeProf.showOnIndex)
                            props.showOnIndex = propertyNodeProf.showOnIndex.toString().toLowerCase();
                        else
                            props.showOnIndex = 'false';

                        if (propertyNodeProf.isHidden)
                            props.isHidden = propertyNodeProf.isHidden.toString().toLowerCase();
                        else
                            props.isHidden = 'false';

                        props.numberOfDecimals = propertyNodeProf.numberOfDecimals;

                        folder.dataProperties.push(props);
                    }
                }
                break;
            case 'Relationships':
                if (!relationFolderNode)
                    break;

                if (relationFolderNode.childNodes)
                    var relChildenNodes = relationFolderNode.childNodes;
                else
                    var relChildenNodes = relationFolderNode.children;

                if (relChildenNodes)
                    for (var k = 0; k < relChildenNodes.length; k++) {
                        var relationNode = relChildenNodes[k];
                        var found = false;
                        for (var ik = 0; ik < folder.dataRelationships.length; ik++)
                            if (relationNode.text.toLowerCase() == folder.dataRelationships[ik].relationshipName.toLowerCase()) {
                                found = true;
                                break;
                            }

                        if (found || relationNode.text == '')
                            continue;

                        if (relationNode.attributes) {
                            if (relationNode.attributes.attributes) {
                                if (relationNode.attributes.attributes.propertyMap)
                                    var relationNodeAttr = relationNode.attributes.attributes;
                                else if (relationNode.attributes.propertyMap)
                                    var relationNodeAttr = relationNode.attributes;
                                else
                                    var relationNodeAttr = relationNode.attributes.attributes;
                            }
                            else {
                                var relationNodeAttr = relationNode.attributes;
                            }
                        }
                        else {
                            relationNodeAttr = relationNode;
                        }

                        var relation = {};
                        relation.propertyMaps = new Array();

                        for (var m = 0; m < relationNodeAttr.propertyMap.length; m++) {
                            var propertyPairNode = relationNodeAttr.propertyMap[m];
                            var propertyPair = {};

                            propertyPair.dataPropertyName = propertyPairNode.dataPropertyName;
                            propertyPair.relatedPropertyName = propertyPairNode.relatedPropertyName;
                            relation.propertyMaps.push(propertyPair);
                        }

                        relation.relatedObjectName = relationNodeAttr.relatedObjectName;
                        relation.relationshipName = relationNodeAttr.text;
                        relation.relationshipType = relationNodeAttr.relationshipTypeIndex;
                        folder.dataRelationships.push(relation);
                    }
                break;
        }
    }
    return folder;
};
function showDialog(width, height, title, message, buttons, callback) {
    if (message.indexOf('\\r\\n') != -1)
        var msg = message.replace('\\r\\n', '\r\n');
    else
        var msg = message;

    if (msg.indexOf("\\") != -1)
        var msgg = msg.replace(/\\\\/g, "\\");
    else
        var msgg = msg;

    if (msg.indexOf("\\u0027") != -1)
        var msgg = msg.replace(/\\u0027/g, "'");
    else
        var msgg = msg;


    var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height + 'px;border:1px solid #aaa;overflow:auto"';
    Ext.Msg.show({
        title: title,
        msg: '<textarea ' + style + ' readonly="yes">' + msgg + '</textarea>',
        buttons: buttons,
        fn: callback
    });
}