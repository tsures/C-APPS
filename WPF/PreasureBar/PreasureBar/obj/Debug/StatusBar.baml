   M S B A M L   `   `   `  ����F  BPreasureBar, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:PreasureBarPreasureBar  5          PreasureBar.StatusBar   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      +localclr-namespace:PreasureBar     5      .��5	      �� 
�������$��1q�6!     �� CornerRadius$  5��65   ._�5
      � .+�5   
   �� -   5         ��Name$ BackgroundRectangle��5      
������ 61   �� 5   
   -   5        $ GradientRectangle��5      $
��Left=�6/   ��5      �� 5       �� 
StartPoint$	 0,0&�6&    �� EndPoint$	 1,0&�67   .��5      �  � Color$ Green��6#    � Offset$	 0.0Q�61   � 5      $ Yellow��6#   $	 0.5Q�62   � 5      $	 Red��6#   $	 1.0Q�6/   5      5      �� 5   
   -   5        $ FilledRectangle��5      
������� J� Opacity$	 0.5Q�6:   �� 5   
   -   5         ��Name$ PercentageLabel��5      $��Center=�5      $��Center=�61   $��26(�5      
�������$	��120��65   $��50��6A   ��5      ��  �� RelativeSource �� 	 �� AncestorType$	 UserControlL�
 �� Path$
 FillPercentage� �� StringFormat$ {0:F1}%�� J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5         
BlurRadius$ 5Q�6#   z�5      �� 5       �� Angle$ 0Q�6&   5       5!      5"      5#      