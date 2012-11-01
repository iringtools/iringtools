Ext.define('AM.view.override.search.SearchTree', {
  override: 'AM.view.search.SearchTree',
  requires:['Ext.ux.plugin.GridPageSizer'],
  initComponent: function () {
    var me = this;
    var storeId = Ext.data.IdGenerator.get("uuid").generate();
      
    me.store = Ext.create('AM.store.SearchStore', {
      storeId: "Search_" + storeId
    });  
    
    me.callOverridden(arguments);
  } 
});