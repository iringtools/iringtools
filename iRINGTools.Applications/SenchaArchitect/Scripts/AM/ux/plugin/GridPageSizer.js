/**
 * @class Ext.ux.plugin.GridPageSizer
 * @extends Ext.Component
 *
 * Creates new PagingToolbarResizer plugin
 * @constructor
 * @param {Object} config The config object
 * 
 */
Ext.define('Ext.ux.plugin.GridPageSizer',  {
  extend: 'Object',
  alias: 'plugin.PagingToolbarResizer',

  options: [5, 10, 15, 20, 25, 30, 50, 75, 100, 200, 300, 500, 1000],
  
  mode: 'remote',
  displayText: 'Records per Page',
  prependCombo: false,
 
  constructor: function(config){	
    Ext.apply(this, config);
    this.callParent();
  },

  init : function(pagingToolbar) {
	
	var comboStore = this.options;

    var combo = new Ext.form.field.ComboBox({
      typeAhead: false,
      triggerAction: 'all',
      forceSelection: true,
      selectOnFocus:true,
      editable: true,
      mode: this.mode,
      value: pagingToolbar.pageSize,
      width:50,
      store: comboStore
  });

    combo.on('select', this.onPageSizeChanged, pagingToolbar);

    var index = 0;
    
    if (this.prependCombo){
      index = pagingToolbar.items.indexOf(pagingToolbar.first);
      index--;
    } else{
      index = pagingToolbar.items.indexOf(pagingToolbar.refresh);
      pagingToolbar.insert(++index,'-');
    }
    
    pagingToolbar.insert(++index, this.displayText);
    pagingToolbar.insert(++index, combo);
    
    if (this.prependCombo){
      pagingToolbar.insert(++index,'-');
    }
    pagingToolbar.on({
      beforedestroy: function(){
      combo.destroy();
      }
    });

  },
  onPageSizeChanged: function (combo) {
    this.store.pageSize = parseInt(combo.getRawValue(), 10);
    this.doRefresh();
  }
});
