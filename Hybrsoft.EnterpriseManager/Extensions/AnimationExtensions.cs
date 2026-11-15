using Hybrsoft.EnterpriseManager.Common;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Numerics;

namespace Hybrsoft.EnterpriseManager.Extensions
{
	static public class AnimationExtensions
	{
		static public void Fade(this UIElement element, double milliseconds, double start, double end, CompositionEasingFunction easingFunction = null)
		{
			element.StartAnimation(nameof(Visual.Opacity), CreateScalarAnimation(milliseconds, start, end, easingFunction));
		}

		static public void Scale(this FrameworkElement element, double milliseconds, double start, double end)
		{
			element.SetCenterPoint(element.ActualWidth / 2.0, element.ActualHeight / 2.0);
			var vectorStart = new Vector3((float)start, (float)start, 0);
			var vectorEnd = new Vector3((float)end, (float)end, 0);
			element.StartAnimation(nameof(Visual.Scale), CreateVector3Animation(milliseconds, vectorStart, vectorEnd));
		}

		static public void Blur(this UIElement element, double milliseconds, double start, double end, CompositionEasingFunction easingFunction = null)
		{
			var brush = CreateBlurEffectBrush();
			element.SetBrush(brush);
			brush.StartAnimation("Blur.BlurAmount", CreateScalarAnimation(milliseconds, start, end, easingFunction));
		}

		static public void Grayscale(this UIElement element)
		{
			var brush = CreateGrayscaleEffectBrush();
			element.SetBrush(brush);
		}

		static public void SetBrush(this UIElement element, CompositionBrush brush)
		{
			var spriteVisual = CreateSpriteVisual(element);
			spriteVisual.Brush = brush;
			ElementCompositionPreview.SetElementChildVisual(element, spriteVisual);
		}

		static public void ClearEffects(this UIElement element)
		{
			ElementCompositionPreview.SetElementChildVisual(element, null);
		}

		static public SpriteVisual CreateSpriteVisual(UIElement element)
		{
			return CreateSpriteVisual(ElementCompositionPreview.GetElementVisual(element));
		}
		static public SpriteVisual CreateSpriteVisual(Visual elementVisual)
		{
			var compositor = elementVisual.Compositor;
			var spriteVisual = compositor.CreateSpriteVisual();
			var expression = compositor.CreateExpressionAnimation();
			expression.Expression = "visual.Size";
			expression.SetReferenceParameter("visual", elementVisual);
			spriteVisual.StartAnimation(nameof(Visual.Size), expression);
			return spriteVisual;
		}

		static public void SetCenterPoint(this UIElement element, double x, double y)
		{
			var visual = ElementCompositionPreview.GetElementVisual(element);
			visual.CenterPoint = new Vector3((float)x, (float)y, 0);
		}

		static public void StartAnimation(this UIElement element, string propertyName, CompositionAnimation animation)
		{
			var visual = ElementCompositionPreview.GetElementVisual(element);
			visual.StartAnimation(propertyName, animation);
		}

		static public CompositionAnimation CreateScalarAnimation(double milliseconds, double start, double end, CompositionEasingFunction easingFunction = null)
		{
			var compositor = GetCurrentCompositor();
			var animation = compositor.CreateScalarKeyFrameAnimation();
			animation.InsertKeyFrame(0.0f, (float)start, easingFunction);
			animation.InsertKeyFrame(1.0f, (float)end, easingFunction);
			animation.Duration = TimeSpan.FromMilliseconds(milliseconds);
			return animation;
		}

		static public CompositionAnimation CreateVector3Animation(double milliseconds, Vector3 start, Vector3 end)
		{
			var compositor = GetCurrentCompositor();
			var animation = compositor.CreateVector3KeyFrameAnimation();
			animation.InsertKeyFrame(0.0f, start);
			animation.InsertKeyFrame(1.0f, end);
			animation.Duration = TimeSpan.FromMilliseconds(milliseconds);
			return animation;
		}

		static public CompositionEffectBrush CreateBlurEffectBrush(double amount = 0.0)
		{
			var effect = new GaussianBlurEffect
			{
				Name = "Blur",
				BlurAmount = (float)amount,
				Source = new CompositionEffectSourceParameter("source")
			};

			var compositor = GetCurrentCompositor();
			var factory = compositor.CreateEffectFactory(effect, (string[])["Blur.BlurAmount"]);
			var brush = factory.CreateBrush();
			brush.SetSourceParameter("source", compositor.CreateBackdropBrush());
			return brush;
		}

		static public CompositionEffectBrush CreateGrayscaleEffectBrush()
		{
			var effect = new GrayscaleEffect
			{
				Name = "Grayscale",
				Source = new CompositionEffectSourceParameter("source")
			};

			var compositor = GetCurrentCompositor();
			var factory = compositor.CreateEffectFactory(effect);
			var brush = factory.CreateBrush();
			brush.SetSourceParameter("source", compositor.CreateBackdropBrush());
			return brush;
		}

		private static Compositor GetCurrentCompositor()
		{
			return WindowTracker.GetCurrentView().Window.Compositor;
		}
	}
}
