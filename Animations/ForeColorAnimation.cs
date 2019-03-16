﻿using System;
using System.Drawing;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter.Animations
{
    internal class ForeColorAnimation : ColorAnimation
    {
        public ForeColorAnimation(DateTime startTime, Color startColor, DateTime endTime, Color endColor)
            : base(startTime, startColor, endTime, endColor)
        {
        }

        public override void Apply(AssLine line, AssSection section, float t)
        {
            section.ForeColor = GetColor(t);
        }
    }
}
