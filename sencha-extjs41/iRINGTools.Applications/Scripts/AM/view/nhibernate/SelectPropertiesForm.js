/*
 * File: Scripts/AM/view/nhibernate/SelectPropertiesForm.js
 *
 * This file was generated by Sencha Architect version 2.1.0.
 * http://www.sencha.com/products/architect/
 *
 * This file requires use of the Ext JS 4.1.x library, under independent license.
 * License of Sencha Architect does not include license for Ext JS 4.1.x. For more
 * details see http://www.sencha.com/license or contact license@sencha.com.
 *
 * This file will be auto-generated each and everytime you save your project.
 *
 * Do NOT hand edit this file.
 */

Ext.define('AM.view.nhibernate.SelectPropertiesForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.selectpropertiesform',

  requires: [
    'AM.view.nhibernate.MultiSelectionGrid'
  ],

  layout: {
    type: 'fit'
  },
  bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'top',
          layout: {
            padding: 4,
            type: 'hbox'
          },
          items: [
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'saveselectproperties',
              iconCls: 'am-apply',
              text: 'Apply'
            },
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'resetselectproperties',
              iconCls: 'am-edit-clear',
              text: 'Reset'
            }
          ]
        }
      ],
      items: [
        {
          xtype: 'multiselectiongrid',
          itemId: 'propertiesSelectionGrid'
        }
      ]
    });

    me.callParent(arguments);
  }

});