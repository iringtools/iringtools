Ext.define('AM.view.override.nhibernate.PropertyGrid', {
  override: 'AM.view.nhibernate.PropertyGrid',
  requires:[
    'AM.model.PropertyMapModel'
  ],
  initComponent: function () {
    var me = this;
    var storeId = Ext.data.IdGenerator.get("uuid").generate();
      
    me.store = Ext.create('Ext.data.ArrayStore', {
      model: 'Am.model.PropertyMapModel',
      data: me.propertyPairs
    });         
    me.callOverridden(arguments);
  } 
  
});