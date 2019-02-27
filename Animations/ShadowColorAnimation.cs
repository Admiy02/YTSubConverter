﻿using System;
using System.Drawing;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    internal class ShadowColorAnimation : ColorAnimation
    {
        public ShadowColorAnimation(DateTime startTime, Color startColor, DateTime endTime, Color endColor)
            : base(startTime, startColor, endTime, endColor)
        {
        }

        public override void Apply(AssDocument.ExtendedLine line, AssDocument.ExtendedSection section, float t)
        {
            section.ShadowColor = GetColor(t);
        }
    }
}
