/*
 * File: Scripts/AM/store/BaseUrlStore.js
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

Ext.define('AM.store.BaseUrlStore', {
  extend: 'Ext.data.Store',

  requires: [
    'AM.model.BaseUrlModel'
  ],

  constructor: function(cfg) {
    var me = this;
    cfg = cfg || {};
    me.callParent([Ext.apply({
      autoLoad: true,
      storeId: 'BaseUrlStore',
      model: 'AM.model.BaseUrlModel',
      proxy: {
        type: 'ajax',
        extraParams: {
          
        },
        timeout: 600000,
        url: 'directory/endpointBaseUrl',
        reader: {
          type: 'json',
          root: 'items'
        }
      }
    }, cfg)]);
  }
});