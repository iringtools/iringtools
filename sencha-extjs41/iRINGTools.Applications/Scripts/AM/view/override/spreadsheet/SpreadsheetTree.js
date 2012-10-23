Ext.define('AM.view.override.spreadsheet.SpreadsheetTree', {
  override: 'AM.view.spreadsheet.SpreadsheetTree',
  initComponent : function() {
    var me = this;
    var storeId = Ext.data.IdGenerator.get("uuid").generate();
      
    me.store = Ext.create('AM.store.SpreadsheetStore', {
      storeId: "Spread_" + storeId
    });         
    me.callOverridden(arguments);
  }
});