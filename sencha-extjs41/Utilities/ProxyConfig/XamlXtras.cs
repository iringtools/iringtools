﻿using System.Windows;
using System.Windows.Controls;

namespace ProxyConfig.XamlXtras
{
  public static class PasswordBoxAssistant
  {
    public static readonly DependencyProperty BoundPassword =
        DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxAssistant), new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static readonly DependencyProperty BindPassword = DependencyProperty.RegisterAttached(
        "BindPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false, OnBindPasswordChanged));

    private static readonly DependencyProperty UpdatingPassword =
        DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxAssistant));

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      PasswordBox box = d as PasswordBox;

      if (d == null || !GetBindPassword(d))
      {
        return;
      }

      box.PasswordChanged -= HandlePasswordChanged;

      string newPassword = (string)e.NewValue;

      if (!GetUpdatingPassword(box))
      {
        box.Password = newPassword;
      }

      box.PasswordChanged += HandlePasswordChanged;
    }

    private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
    {
      PasswordBox box = dp as PasswordBox;

      if (box == null)
      {
        return;
      }

      bool wasBound = (bool)(e.OldValue);
      bool needToBind = (bool)(e.NewValue);

      if (wasBound)
      {
        box.PasswordChanged -= HandlePasswordChanged;
      }

      if (needToBind)
      {
        box.PasswordChanged += HandlePasswordChanged;
      }
    }

    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
      PasswordBox box = sender as PasswordBox;

      SetUpdatingPassword(box, true);
      SetBoundPassword(box, box.Password);
      SetUpdatingPassword(box, false);
    }

    public static void SetBindPassword(DependencyObject dp, bool value)
    {
      dp.SetValue(BindPassword, value);
    }

    public static bool GetBindPassword(DependencyObject dp)
    {
      return (bool)dp.GetValue(BindPassword);
    }

    public static string GetBoundPassword(DependencyObject dp)
    {
      return (string)dp.GetValue(BoundPassword);
    }

    public static void SetBoundPassword(DependencyObject dp, string value)
    {
      dp.SetValue(BoundPassword, value);
    }

    private static bool GetUpdatingPassword(DependencyObject dp)
    {
      return (bool)dp.GetValue(UpdatingPassword);
    }

    private static void SetUpdatingPassword(DependencyObject dp, bool value)
    {
      dp.SetValue(UpdatingPassword, value);
    }
  }
}
