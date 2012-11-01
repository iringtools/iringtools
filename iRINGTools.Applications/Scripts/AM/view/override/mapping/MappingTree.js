Ext.define('AM.view.override.mapping.MappingTree', {
  override: 'AM.view.mapping.MappingTree',
  initComponent : function() {
    var me = this;
    var storeId = Ext.data.IdGenerator.get("uuid").generate();
      
    me.store = Ext.create('AM.store.MappingStore', {
      storeId: "Mapping_" + storeId
    });   
    
    me.callOverridden(arguments);
  }
  
});