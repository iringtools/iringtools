Ext.define('AM.view.override.directory.DataGridPanel', {
  override: 'AM.view.directory.DataGridPanel',
  initComponent: function () {
    var me = this;
     var storeId = Ext.data.IdGenerator.get("uuid").generate();
      
    me.store = Ext.create('AM.store.DataGridStore', {
      storeId: "DataGrid" + storeId
    });
    
    var ptb = Ext.create('Ext.PagingToolbar', {
      pageSize: 25,
      store: me.store,
      displayInfo: true,
      displayMsg: 'Records {0} - {1} of {2}',
      emptyMsg: "No records to display",
      plugins: [Ext.create('Ext.ux.plugin.GridPageSizer', { options: [25, 50, 100, 200] })]
    });
    
    var filters = {
      ftype: 'filters',
      local: false,
      buildQuery: function (filters) {
        var processed_filters = [];

        for (var i = 0; i < filters.length; i++) {
          var pf = {};
          var filter = filters[i];
          pf.field = filter.field;

          if (filter.data.type == 'numeric') {
            pf.comparison = filter.data.comparison;
            pf.value = filter.data.value;
            pf.type = filter.data.type;
          }
          else {
            for (var key in filter.data) {
              pf[key] = filter.data[key];
            }
            pf.comparison = 'eq';
          }

          processed_filters.push(pf);

        }
        return { filter: Ext.encode(processed_filters) };
      }
    };
    
    Ext.apply(me, {
      bbar: ptb,
      iconCls: 'tabsData',
      columns: {
        defaults: {
          field: { xtype: 'textfield' }
        }
      },
      features: [filters]
    });
    
  me.callOverridden(arguments);
  } 
});