var ifExistSibling = function (str, node, state) {
  var ifExist = false;
  var childNodes = node.childNodes;
  
  if(!childNodes) return ifExist;
  var repeatTime = 0;

  for (var i = 0; i < childNodes.length; i++) {
    if (childNodes[i].data.text == str) {
      if (state == 'new')
        ifExist = true;
      else {
        repeatTime++;
        if (repeatTime > 1)
          ifExist = true;
      }
    }
  }

  return ifExist;
};


String.format = String.prototype.format = function () {
  var i = 0;
  var string = (typeof (this) == "function" && !(i++)) ? arguments[0] : this;
  for (; i < arguments.length; i++)
    string = string.replace(/\{\d+?\}/, arguments[i]);
  return string;
};  


function showDialog(width, height, title, msg, buttons, callback) {
  while (msg.indexOf('\\r\\n') != -1)
    msg = msg.replace('\\r\\n', ' \r\n');

  var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height + 'px;border:1px solid #aaa;overflow:auto"';
  Ext.Msg.show({
    title: title,
    msg: '<textarea ' + style + ' readonly="yes">' + msg + '</textarea>',
    buttons: buttons,
    fn: callback
  });
}

function getLastXString(str, num) {
  var index = str.length;

  if (str[index - 1] == '.')
    str = str.substring(0, index - 1);

  for (var i = 0; i < num; i++) {
    str = str.substring(0, index);
    index = str.lastIndexOf('/');
  }
  return str.substring(index + 1);
}


///overrides required to display correct text on dragstart

Ext.override(Ext.view.DragZone, {
  getDragText: function () {
    if (this.dragField) {
      var fieldValue = this.dragData.records[0].get(this.dragField);
      return Ext.String.format(this.dragText, fieldValue);
    } else {
      var count = this.dragData.records.length;
      return Ext.String.format(this.dragText, count, count == 1 ? '' : 's');
    }
  }
});

Ext.override(Ext.tree.plugin.TreeViewDragDrop, {
  onViewRender: function (view) {
    var me = this;
    if (me.enableDrag) {
      me.dragZone = Ext.create('Ext.tree.ViewDragZone', {
        view: view,
        ddGroup: me.dragGroup || me.ddGroup,
        dragText: me.dragText,
        dragField: me.dragField,
        repairHighlightColor: me.nodeHighlightColor,
        repairHighlight: me.nodeHighlightOnRepair
      });
    }
    if (me.enableDrop) {
      me.dropZone = Ext.create('Ext.tree.ViewDropZone', {
        view: view,
        ddGroup: me.dropGroup || me.ddGroup,
        allowContainerDrops: me.allowContainerDrops,
        appendOnly: me.appendOnly,
        allowParentInserts: me.allowParentInserts,
        expandDelay: me.expandDelay,
        dropHighlightColor: me.nodeHighlightColor,
        dropHighlight: me.nodeHighlightOnDrop
      });
    }
  }
});